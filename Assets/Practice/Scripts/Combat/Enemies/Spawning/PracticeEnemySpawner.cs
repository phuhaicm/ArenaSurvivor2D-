using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PracticeEnemySpawner : HaiMonoBehaviour
{
    [Header("Enemy Type Reference")]
    [SerializeField] private PracticeEnemyTypeData[] enemyTypes;
    [Tooltip("Prefab Quái Tinh Anh (Elite) to khỏe và nổ ra mưa hạt XP. Nếu để trống sẽ tự lấy quái cuối mảng enemyTypes.")]
    [SerializeField] private PracticeEnemyHealth eliteEnemyPrefab;
    private readonly List<PracticeEnemyTypeData> availableEnemyTypes = new();
    private int currentDifficulyLevel = 1;
    [SerializeField] private Transform enemyContainer;

    [Header("Player & Run References")]
    [SerializeField] protected PracticePlayerHealth playerHealth;
    [SerializeField] private PracticeRunDirector runDirector;

    [Header("World References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform playerTarget;

    [Header("Spawn Timing")]
    [SerializeField, Min(0f)] private float initalSpawnDelay = 1f;
    [SerializeField, Min(0.1f)] private float spawnInterval = 2f;

    [Header("Spawn Limits")]
    [SerializeField, Min(1)] private int maxAliveEnemies = 10;
    [SerializeField, Min(0f)] private float spawnPadding = 1.5f;
    [SerializeField, Min(0f)] private float miniumDistanceFromPlayer = 3f;
    [SerializeField, Min(1)] private int maxiumSpawnAttemps = 8;

    [Header("Difficulty Reference")]
    [SerializeField] private PracticeDifficultySystem difficultySystem;

    [Header("Debug")]
    [SerializeField, Min(0.05f)] private float spawnPointGizmoRadius = 0.2f;
    [SerializeField] private bool enableVerboseSpawnLogs = false;

    private readonly List<PracticeEnemyHealth> spawnedEnemies = new();
    private float nextSpawnTime;

    [SerializeField, Min(1)] private int batchSize = 1;
    [SerializeField, Min(0f)] private float delayBetweenBatchEnemies = 0.2f;
    private Coroutine spawnBatchCoroutine;

    public event Action EnemyDefeated;

    private bool canSpawn = true;
    private bool hasStarted;

    public int AliveEnemyCount => spawnedEnemies.Count;
    public int MaxAliveEnemies => maxAliveEnemies;
    public float SpawnInterval => spawnInterval;
    public int BatchSize => batchSize;
    public Transform EnemyContainer => enemyContainer;
    public bool CanSpawn => canSpawn;
    public bool HasStarted => hasStarted;

    protected override void Start()
    {
        base.Start();
        canSpawn = runDirector == null || runDirector.CurrentPhase == PracticeRunPhase.Survival;
        hasStarted = true;
        nextSpawnTime = Time.time + initalSpawnDelay;
    }

    private void Update()
    {
        if (!canSpawn) return;
        if (spawnBatchCoroutine != null) return;
        if (Time.time < nextSpawnTime) return;

        RemoveDestroyedEnemies();

        if (spawnedEnemies.Count >= maxAliveEnemies) return;
        spawnBatchCoroutine = StartCoroutine(SpawnBatchCoroutine());
    }

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadPlayerHealth();
        LoadRunDirector();
        LoadWorldReferences();
    }

    protected void LoadPlayerHealth()
    {
        if (playerHealth != null) return;
        playerHealth = FindFirstObjectByType<PracticePlayerHealth>();
    }

    private void LoadRunDirector()
    {
        if (runDirector != null) return;
        runDirector = FindFirstObjectByType<PracticeRunDirector>();
    }

    private void LoadWorldReferences()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (playerTarget == null && playerHealth != null)
        {
            playerTarget = playerHealth.transform;
        }

        if (enemyContainer == null)
        {
            GameObject containerObj = GameObject.Find("RuntimeEnemies");
            if (containerObj != null)
            {
                enemyContainer = containerObj.transform;
            }
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        LoadPlayerHealth();
        LoadRunDirector();
        LoadWorldReferences();

        LoadDifficultyEvents();
        SubscribeAllEnemyEvents();
        SubcribePlayerEvents();
        SubscribeRunDirectorEvents();
    }

    protected override void OnDisable()
    {
        UnLoadDifficultyEvents();
        UnsubscribeAllEnemyEvents();
        StopSpawnBatch();
        UnSubcribePlayerEvents();
        UnsubscribeRunDirectorEvents();
        base.OnDisable();
    }

    protected void SubcribePlayerEvents()
    {
        if (playerHealth == null) return;
        playerHealth.Died -= HandlePlayerDied;
        playerHealth.Died += HandlePlayerDied;
    }

    protected void UnSubcribePlayerEvents()
    {
        if (playerHealth == null) return;
        playerHealth.Died -= HandlePlayerDied;
    }

    private void SubscribeRunDirectorEvents()
    {
        if (runDirector == null) return;
        runDirector.RunPhaseChanged -= HandleRunPhaseChanged;
        runDirector.RunPhaseChanged += HandleRunPhaseChanged;
    }

    private void UnsubscribeRunDirectorEvents()
    {
        if (runDirector == null) return;
        runDirector.RunPhaseChanged -= HandleRunPhaseChanged;
    }

    private void HandleRunPhaseChanged(PracticeRunPhase newPhase)
    {
        // Chỉ cho phép sinh quái thường khi đang ở pha Survival.
        // Khi bước sang BossFight hoặc Completed, tự động khóa Spawner và ngắt Coroutine ngay lập tức.
        SetSpawningEnabled(newPhase == PracticeRunPhase.Survival);
    }

    protected void LoadDifficultyEvents()
    {
        if (difficultySystem == null) return;
        difficultySystem.DifficultyChanged -= HandleDifficultyChanged;
        difficultySystem.DifficultyChanged += HandleDifficultyChanged;
    }

    protected void UnLoadDifficultyEvents()
    {
        if (difficultySystem != null)
        {
            difficultySystem.DifficultyChanged -= HandleDifficultyChanged;
        }
    }

    protected void SubscribeAllEnemyEvents()
    {
        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            PracticeEnemyHealth enemy = spawnedEnemies[i];
            if (enemy == null) continue;
            enemy.Died -= HandleEnemyDied;
            enemy.Died += HandleEnemyDied;
        }
    }

    protected void UnsubscribeAllEnemyEvents()
    {
        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            PracticeEnemyHealth enemy = spawnedEnemies[i];
            if (enemy == null) continue;
            enemy.Died -= HandleEnemyDied;
        }
    }

    protected void HandlePlayerDied()
    {
        SetSpawningEnabled(false);
    }

    private bool TrySelectEnemyType(out PracticeEnemyTypeData selectedEnemyType)
    {
        BuilAvailableEnemyTypes();

        if (availableEnemyTypes.Count == 0)
        {
            selectedEnemyType = null;
            return false;
        }

        float totalWeight = CalculateTotalWeight();

        if (totalWeight <= 0f)
        {
            selectedEnemyType = null;
            return false;
        }

        selectedEnemyType = SelectedEnemyTypeByWeight(totalWeight);

        if (enableVerboseSpawnLogs && selectedEnemyType != null)
        {
            Debug.Log(
                $"Selected Enemy Type: {selectedEnemyType.EnemyTypeName} | Weight: {selectedEnemyType.SpawnWeight}"
            );
        }

        return selectedEnemyType != null;
    }

    private void BuilAvailableEnemyTypes()
    {
        availableEnemyTypes.Clear();

        if (enemyTypes == null) return;

        for (int i = 0; i < enemyTypes.Length; i++)
        {
            PracticeEnemyTypeData enemyType = enemyTypes[i];
            if (enemyType == null) continue;
            if (enemyType.EnemyPrefab == null) continue;

            bool isUnlock = enemyType.UnlockDifficultyLevel <= currentDifficulyLevel;
            if (!isUnlock) continue;

            if (enemyType.SpawnWeight <= 0f) continue;

            availableEnemyTypes.Add(enemyType);
        }
    }

    private float CalculateTotalWeight()
    {
        float totalWeight = 0f;

        for (int i = 0; i < availableEnemyTypes.Count; i++)
        {
            PracticeEnemyTypeData enemyType = availableEnemyTypes[i];
            if (enemyType == null) continue;
            totalWeight += enemyType.SpawnWeight;
        }

        return totalWeight;
    }

    private PracticeEnemyTypeData SelectedEnemyTypeByWeight(float totalWeight)
    {
        float randomIndex = Random.Range(0f, totalWeight);
        float cucumlativeWeight = 0f;

        for (int i = 0; i < availableEnemyTypes.Count; i++)
        {
            PracticeEnemyTypeData enemyType = availableEnemyTypes[i];
            cucumlativeWeight += enemyType.SpawnWeight;

            if (randomIndex <= cucumlativeWeight)
            {
                return enemyType;
            }
        }

        return availableEnemyTypes[availableEnemyTypes.Count - 1];
    }

    private void StopSpawnBatch()
    {
        if (spawnBatchCoroutine == null)
        {
            return;
        }

        StopCoroutine(spawnBatchCoroutine);
        spawnBatchCoroutine = null;
    }

    private IEnumerator SpawnBatchCoroutine()
    {
        for (int i = 0; i < batchSize; i++)
        {
            if (!CanSpawn) break;

            RemoveDestroyedEnemies();

            if (spawnedEnemies.Count >= maxAliveEnemies) break;

            bool spawnedSuccessfully = TrySpawnSingleEnemy();

            if (!spawnedSuccessfully) break;

            bool spawnEndBatch = i < batchSize - 1;

            if (spawnEndBatch && delayBetweenBatchEnemies > 0f)
            {
                yield return new WaitForSeconds(delayBetweenBatchEnemies);
            }
        }

        ScheduleNextSpawn();
        spawnBatchCoroutine = null;
    }

    private void HandleDifficultyChanged(PracticeDifficultyData difficultyData)
    {
        currentDifficulyLevel = difficultyData.Level;
        spawnInterval = difficultyData.SpawnInterval;
        maxAliveEnemies = difficultyData.MaxAliveEnemies;
        batchSize = difficultyData.BatchSize;

        if (hasStarted)
        {
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    protected void HandleEnemyDied()
    {
        PracticeAudioManager.Instance?.PlayEnemyDeathSfx();
        EnemyDefeated?.Invoke();
    }

    private bool TrySpawnSingleEnemy()
    {
        return SpawnEnemyInternal(false, out _);
    }

    public PracticeEnemyHealth SpawnBenchmarkEnemy()
    {
        SpawnEnemyInternal(true, out PracticeEnemyHealth spawned);
        return spawned;
    }

    private bool SpawnEnemyInternal(bool ignoreUnlockLevel, out PracticeEnemyHealth spawnedEnemy)
    {
        spawnedEnemy = null;

        if (!HasValidSpawnSetup())
        {
            return false;
        }

        if (!TryGetSpawnPosition(out Vector2 spawnPosition))
        {
            spawnPosition = GenerateSpawnPosition();
        }

        PracticeEnemyTypeData selectedEnemyType = null;

        if (ignoreUnlockLevel && enemyTypes != null && enemyTypes.Length > 0)
        {
            int randomIdx = Random.Range(0, enemyTypes.Length);
            selectedEnemyType = enemyTypes[randomIdx];
        }

        if (selectedEnemyType == null || selectedEnemyType.EnemyPrefab == null)
        {
            if (!TrySelectEnemyType(out selectedEnemyType))
            {
                return false;
            }
        }

        PracticeEnemyHealth selectedPrefab = selectedEnemyType.EnemyPrefab;

        spawnedEnemy =
            PracticeObjectPoolManager.SpawnOrInstantiate(
                selectedPrefab,
                spawnPosition,
                Quaternion.identity,
                enemyContainer
            );

        if (spawnedEnemy == null)
        {
            return false;
        }

        if (enableVerboseSpawnLogs)
        {
            Debug.Log(
                $"SPAWN | Enemy: {spawnedEnemy.name} | Pos: {spawnPosition}",
                spawnedEnemy
            );
        }

        if (!spawnedEnemies.Contains(spawnedEnemy))
        {
            spawnedEnemies.Add(spawnedEnemy);
        }

        spawnedEnemy.Died -= HandleEnemyDied;
        spawnedEnemy.Died += HandleEnemyDied;

        return true;
    }
    public PracticeEnemyHealth SpawnEliteEnemy()
    {
        if (!HasValidSpawnSetup())
        {
            return null;
        }

        PracticeEnemyHealth prefabToSpawn = eliteEnemyPrefab;

        if (prefabToSpawn == null && enemyTypes != null && enemyTypes.Length > 0)
        {
            // Fallback an toàn: lấy loại quái mạnh nhất ở cuối danh sách (thường là Tank)
            for (int i = enemyTypes.Length - 1; i >= 0; i--)
            {
                if (enemyTypes[i] != null && enemyTypes[i].EnemyPrefab != null)
                {
                    prefabToSpawn = enemyTypes[i].EnemyPrefab;
                    break;
                }
            }
        }

        if (prefabToSpawn == null)
        {
            return null;
        }

        if (!TryGetSpawnPosition(out Vector2 spawnPosition))
        {
            spawnPosition = GenerateSpawnPosition();
        }

        PracticeEnemyHealth spawnedElite =
            PracticeObjectPoolManager.SpawnOrInstantiate(
                prefabToSpawn,
                spawnPosition,
                Quaternion.identity,
                enemyContainer
            );

        if (spawnedElite == null)
        {
            return null;
        }

        // Nếu dùng tạm Prefab thường chưa chỉnh dropCount trên Inspector, tự động cấu hình nổ 10 hạt XP
        PracticeXPDropOnDeath xpDrop = spawnedElite.GetComponent<PracticeXPDropOnDeath>();
        if (xpDrop != null && eliteEnemyPrefab == null)
        {
            xpDrop.ConfigureDropBurst(10, 1.25f);
        }

        RegisterSpawnedEnemy(spawnedElite);
        return spawnedElite;
    }

    public int SpawnAmbushRing(int ringEnemyCount, float ringRadius = 7.5f)//lấy vị trí hiện tại của player
    {
        if (!HasValidSpawnSetup() || playerTarget == null)
        {
            return 0;
        }

        PracticeEnemyHealth ringPrefab = null;

        if (enemyTypes != null && enemyTypes.Length > 0 && enemyTypes[0] != null)
        {
            ringPrefab = enemyTypes[0].EnemyPrefab;
        }

        if (ringPrefab == null)
        {
            return 0;
        }

        int validCount = Mathf.Clamp(ringEnemyCount, 4, 60);
        float validRadius = Mathf.Max(3.5f, ringRadius);
        float angleStep = 360f / validCount;
        Vector2 center = playerTarget.position;
        int spawnedCount = 0;

        for (int i = 0; i < validCount; i++)
        {
            float angleRad = (angleStep * i) * Mathf.Deg2Rad;
            Vector2 spawnPos = center + new Vector2(
                Mathf.Cos(angleRad) * validRadius,
                Mathf.Sin(angleRad) * validRadius
            );

            PracticeEnemyHealth enemy =
                PracticeObjectPoolManager.SpawnOrInstantiate(
                    ringPrefab,
                    spawnPos,
                    Quaternion.identity,
                    enemyContainer
                );

            if (enemy != null)
            {
                RegisterSpawnedEnemy(enemy);
                spawnedCount++;
            }
        }

        return spawnedCount;
    }

    private void RegisterSpawnedEnemy(PracticeEnemyHealth spawnedEnemy)
    {
        if (spawnedEnemy == null) return;

        if (!spawnedEnemies.Contains(spawnedEnemy))
        {
            spawnedEnemies.Add(spawnedEnemy);
        }

        spawnedEnemy.Died -= HandleEnemyDied;
        spawnedEnemy.Died += HandleEnemyDied;
    }

    private bool TryGetSpawnPosition(out Vector2 spawnPosition)
    {
        for (int attempt = 0; attempt < maxiumSpawnAttemps; attempt++)
        {
            Vector2 candidatePosition = GenerateSpawnPosition();
            if (IsFarEnoughFromPlayer(candidatePosition))
            {
                spawnPosition = candidatePosition;
                return true;
            }
        }

        spawnPosition = Vector2.zero;
        return false;
    }

    private Vector2 GenerateSpawnPosition()
    {
        float gamePlayPlaneZ = 0f;
        float distanceFromCameraToPlane = Mathf.Abs(gamePlayPlaneZ - mainCamera.transform.position.z);

        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, distanceFromCameraToPlane));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, distanceFromCameraToPlane));

        float cameraleft = bottomLeft.x;
        float cameraRight = topRight.x;
        float cameraBottom = bottomLeft.y;
        float cameraTop = topRight.y;

        SpawnEdge selectedEdge = (SpawnEdge)Random.Range(0, 4);
        switch (selectedEdge)
        {
            case SpawnEdge.Top:
            {
                float randomX = Random.Range(cameraleft, cameraRight);
                return new Vector2(randomX, cameraTop + spawnPadding);
            }
            case SpawnEdge.Bottom:
            {
                float randomX = Random.Range(cameraleft, cameraRight);
                return new Vector2(randomX, cameraBottom - spawnPadding);
            }
            case SpawnEdge.Left:
            {
                float randomY = Random.Range(cameraBottom, cameraTop);
                return new Vector2(cameraleft - spawnPadding, randomY);
            }
            case SpawnEdge.Right:
            {
                float randomY = Random.Range(cameraBottom, cameraTop);
                return new Vector2(cameraRight + spawnPadding, randomY);
            }
            default:
                return Vector2.zero;
        }
    }

    private bool IsFarEnoughFromPlayer(Vector2 spawnPosition)
    {
        if (playerTarget == null)
        {
            return true;
        }

        Vector2 offset = spawnPosition - (Vector2)playerTarget.position;
        return offset.sqrMagnitude >= (miniumDistanceFromPlayer * miniumDistanceFromPlayer);
    }

    private void ScheduleNextSpawn()
    {
        nextSpawnTime = Time.time + spawnInterval;
    }

    public void RemoveDestroyedEnemies()
    {
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            PracticeEnemyHealth enemy = spawnedEnemies[i];

            if (enemy == null || enemy.IsDead || !enemy.gameObject.activeInHierarchy)
            {
                if (enemy != null)
                {
                    enemy.Died -= HandleEnemyDied;
                }

                spawnedEnemies.RemoveAt(i);
            }
        }
    }

    private bool HasValidSpawnSetup()
    {
        if (enemyTypes == null || enemyTypes.Length == 0)
        {
            Debug.LogError($"{name}: Chua duoc gan enemy type data.", this);
            return false;
        }

        if (mainCamera == null)
        {
            Debug.LogError($"{name}: chua co gan main Camera.", this);
            return false;
        }

        if (playerTarget == null)
        {
            Debug.LogError($"{name}: chua co gan player target.", this);
            return false;
        }

        return true;
    }

    public void SetSpawningEnabled(bool isEnabled)
    {
        canSpawn = isEnabled;

        if (!canSpawn)
        {
            StopSpawnBatch();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (mainCamera == null) return;

        float gamePlayPlaneZ = 0f;
        float distanceFromCameraToPlane = Mathf.Abs(gamePlayPlaneZ - mainCamera.transform.position.z);

        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, distanceFromCameraToPlane));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, distanceFromCameraToPlane));

        float left = bottomLeft.x - spawnPadding;
        float right = topRight.x + spawnPadding;
        float bottom = bottomLeft.y - spawnPadding;
        float top = topRight.y + spawnPadding;

        Vector3 spawnAreaCenter = new Vector3((left + right) * 0.5f, (bottom + top) * 0.5f, gamePlayPlaneZ);
        Vector3 spawnAreaSize = new Vector3(right - left, top - bottom, 0f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(spawnAreaCenter, spawnAreaSize);

        Gizmos.DrawWireSphere(new Vector3(left, top, gamePlayPlaneZ), spawnPointGizmoRadius);
        Gizmos.DrawWireSphere(new Vector3(right, top, gamePlayPlaneZ), spawnPointGizmoRadius);
        Gizmos.DrawWireSphere(new Vector3(left, bottom, gamePlayPlaneZ), spawnPointGizmoRadius);
        Gizmos.DrawWireSphere(new Vector3(right, bottom, gamePlayPlaneZ), spawnPointGizmoRadius);
    }
}