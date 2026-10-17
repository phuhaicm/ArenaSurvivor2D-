using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class PracticeEnemySpawner : HaiMonoBehaviour
{
    [Header("Enemy Type Reference")] 
    [SerializeField] private PracticeEnemyTypeData[] enemyTypes;
    private readonly List<PracticeEnemyTypeData>
        availableEnemyTypes = new();
    private int currentDifficulyLevel = 1;
    [SerializeField] private Transform enemyContainer;
    [Header("Playr Reference")] 
    [SerializeField] protected PracticePlayerHealth playerHealth;
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
    private readonly List<PracticeEnemyHealth>//kko thể gắn sang 1 list khác
        spawnedEnemies = new();// nếu ko có dòng này thì khi add enemy vào thì sẽ bị null
    private float nextSpawnTime;
    //public event Action<PracticeDifficultyData>
    [SerializeField, Min(1)] private int batchSize = 1;
    [SerializeField, Min(0f)] private float delayBetweenBatchEnemies = 0.2f;
    private Coroutine spawnBatchCoroutine;// check xem batch có hoạt động ko đặt về null khi hoàn thành
    public event Action EnemyDefeated;
    private bool canSpawn = true;
    private bool hasStarted;
    public int AliveEnemyCount => spawnedEnemies.Count;// để script khác bên ngoài đọc được
    public bool CanSpawn => canSpawn;
    public bool HasStarted => hasStarted;
    protected override void Start()
    {
        base.Start();
        canSpawn = true;
        hasStarted = true;
        nextSpawnTime = Time.time + initalSpawnDelay;
    }
    private void Update()
    {
        if(!canSpawn) return;
        if(spawnBatchCoroutine != null) return;
        if(Time.time < nextSpawnTime) return;

        RemoveDestroyedEnemies();

        if(spawnedEnemies.Count >= maxAliveEnemies) return;
        spawnBatchCoroutine= StartCoroutine(SpawnBatchCoroutine());//bắt đầu 1 tiến trình batch
    }
    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadPlayerHealth();
    }
    protected void LoadPlayerHealth()
    {
        if(playerHealth != null) return;
        playerHealth = FindFirstObjectByType<PracticePlayerHealth>();
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        LoadDifficultyEvents();
        SubscribeAllEnemyEvents();
        SubcribePlayerEvents();
    }
    protected override void OnDisable()
    {
        UnLoadDifficultyEvents();
        UnsubscribeAllEnemyEvents();
        StopSpawnBatch();//trước đây ko cần dừng vì mỗi lần spawn chạy xong ngay trong một method có thể đang ngủ tại yield return nên cần cleanup
        UnSubcribePlayerEvents();
        base.OnDisable();
    }
    protected void SubcribePlayerEvents()
    {
        if(playerHealth == null) return;
        playerHealth.Died += HandlePlayerDied;
    }
    protected void LoadDifficultyEvents()
    {
        if(difficultySystem == null) return;

        difficultySystem.DifficultyChanged += HandleDifficultyChanged;
    }
    protected void UnLoadDifficultyEvents()
    {
        if(difficultySystem != null)
        {
            difficultySystem.DifficultyChanged -= HandleDifficultyChanged;
        }
    }
    protected void UnSubcribePlayerEvents()
    {
        if(playerHealth == null) return;
        playerHealth.Died -= HandlePlayerDied;
    }
    protected void SubscribeAllEnemyEvents()
    {
        foreach(PracticeEnemyHealth enemy in spawnedEnemies)
        {
            if(enemy == null) continue;
            enemy.Died -= HandleEnemyDied;
            enemy.Died += HandleEnemyDied;
        }
    }
    protected void UnsubscribeAllEnemyEvents()
    {
        foreach(PracticeEnemyHealth enemy in spawnedEnemies)
        {
            if(enemy == null) continue;
            enemy.Died -= HandleEnemyDied;
        }
    }
    protected void HandlePlayerDied()
    {
        SetSpawningEnabled(false);
    }
    private bool TrySelectEnemyType(out PracticeEnemyTypeData selectedEnemyType)// practiceenemytypedata selec.. là biến selec sẽ tham chiếu để kiểu dữ liệu của practce.. do đó nó có thể bằng 3 assest fast tank normal và null 
    //out đơn giản là gửi kết quả ra bên ngoài
    {
        BuilAvailableEnemyTypes();

        if(availableEnemyTypes.Count == 0)
        {
            selectedEnemyType = null;//do có out nên phải có dòng này chứ ko là lỗi tham chiếu
            return false;
        }
        float totalWeight = CalculateTotalWeight();

        if(totalWeight <= 0)
        {
            selectedEnemyType = null;
            return false;
        }

        selectedEnemyType = SelectedEnemyTypeByWeight(totalWeight);

        Debug.Log(
            $"Selected Enemy Type: " +
            $"{selectedEnemyType.EnemyTypeName} | " +
            $"Weight: {selectedEnemyType.SpawnWeight}"
        );

        return selectedEnemyType != null;// có nghĩa là khác null là true == null là false
    }
    private void BuilAvailableEnemyTypes()//cú pháp này có nghĩa gửi enemy type ra ngoài out là gửi
    {
        availableEnemyTypes.Clear();

        foreach(PracticeEnemyTypeData enemyType in enemyTypes)
        {
            if(enemyType == null) continue;
            if(enemyType.EnemyPrefab == null) continue;

            bool isUnlock = enemyType.UnlockDifficultyLevel <= currentDifficulyLevel;
            if(!isUnlock) continue;

            if(enemyType.SpawnWeight <= 0f) continue;

            availableEnemyTypes.Add(enemyType);
        }
    }
    private float CalculateTotalWeight()
    {
        float totalWeight = 0f;
        
        foreach(PracticeEnemyTypeData enemyType in availableEnemyTypes)
        {
            if(enemyType == null) continue;
            totalWeight += enemyType.SpawnWeight;
        }
        return totalWeight;
    }
    private PracticeEnemyTypeData SelectedEnemyTypeByWeight(float totalWeight)// gọi là kiểu trả về int là nguyên còn này là enemytype
    {
        float randomIndex = Random.Range(0,totalWeight);

        float cucumlativeWeight = 0f;
        
        foreach(PracticeEnemyTypeData enemyType in availableEnemyTypes)
        {
            cucumlativeWeight += enemyType.SpawnWeight;;

            if(randomIndex <= cucumlativeWeight)
            return enemyType;
        }
        return availableEnemyTypes[availableEnemyTypes.Count - 1];//thêm để chống lỗi nếu vì sai số mà ko chọn được phần tử trong foreach thì trả về phần tử cuối cùng
    }
    private void StopSpawnBatch()
    {
        if(spawnBatchCoroutine == null)
        {
            return;
        }
        StopCoroutine(spawnBatchCoroutine);
        spawnBatchCoroutine = null;
    }
    private IEnumerator SpawnBatchCoroutine()
    {
        for(int i = 0; i < batchSize; i++)
        {
            if(!CanSpawn) break;//spawner bị khóa 

            RemoveDestroyedEnemies();

            if(spawnedEnemies.Count >= maxAliveEnemies) break;

            bool spawnedSuccessfully = TrySpawnSingleEnemy();
            
            if(!spawnedSuccessfully) break;// spawn thât bại thì bnreak

            bool spawnEndBatch = i < batchSize -1;

            if (spawnEndBatch)
            {
                yield return new WaitForSeconds(delayBetweenBatchEnemies);
            }
        }
        ScheduleNextSpawn();
        spawnBatchCoroutine = null; //xóa reference
    }
    private void HandleDifficultyChanged(PracticeDifficultyData difficultyData)
    {
        //if(difficultyData == null) return;

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
        if(!HasValidSpawnSetup())
        {
            return false;
        }

        if(!TryGetSpawnPosition(out Vector2 spawnPosition))
        {
            Debug.LogWarning($"{name}: khong tim duoc vi tri spawn hop le",this);
            return false;
        }   
        if(!TrySelectEnemyType(out PracticeEnemyTypeData selectedEnemyType))
        {
            Debug.LogWarning($"{name}: khong co enemy type " + " phu hop voi difficulty hien tai. ",this);
            return false;
        }
        PracticeEnemyHealth selectedPrefab = selectedEnemyType.EnemyPrefab;
        PracticeEnemyHealth spawnedEnemy = Instantiate(
        selectedPrefab,
        spawnPosition,
        Quaternion.identity,
        enemyContainer
    );

Debug.Log(
    $"SPAWN | Enemy: {spawnedEnemy.name} | " +
    $"Spawn Position: {spawnPosition} | " +
    $"Actual Position: {spawnedEnemy.transform.position} | " +
    $"Player Position: {playerTarget.position} | " +
    $"Distance: {Vector2.Distance(spawnedEnemy.transform.position, playerTarget.position):F2}",
    spawnedEnemy
);

    spawnedEnemies.Add(spawnedEnemy);
    spawnedEnemy.Died += HandleEnemyDied;

        return true;
    }
    private bool TryGetSpawnPosition(out Vector2 spawnPosition)
    {
        for(int attempt = 0; attempt < maxiumSpawnAttemps; attempt++)
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
        if(playerTarget == null)
        {
            return true;
        }
        //trả về tryu là vị trí hợp lệ đủ xa player
        float distanceFromPlayer = Vector2.Distance(spawnPosition, playerTarget.position);

        return distanceFromPlayer >= miniumDistanceFromPlayer;
    }
    private void ScheduleNextSpawn()
    {
        nextSpawnTime = Time.time + spawnInterval;
    }
    private void RemoveDestroyedEnemies()
    {
        for(int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if(spawnedEnemies[i] == null)
            spawnedEnemies.RemoveAt(i);
        }
        /// <summary>
        // phải dùng từ dưới lên bởi vì sẽ ko bị đảo lộn vị trí index khi mà remove rồi tiếp tục dò
        /// </summary>
    }
    private bool HasValidSpawnSetup()
    {
        if(enemyTypes == null || enemyTypes.Length == 0)
        {
            Debug.LogError(
                $"{name}: Chua duoc gan enemy type data.", this
            );
            return false;
        }
        if(mainCamera == null)
        {
            Debug.LogError(
                $"{name}: chua co gan main Camera.", this
            );
            return false;
        }
        if(playerTarget == null)
        {
            Debug.LogError(
                $"{name}: chua co gan player target. ",this
            );
            return false;
        }
        return true;
    }
    private void SetSpawningEnabled(bool isEnabled)
    {
        canSpawn = isEnabled;
        
        if (!canSpawn)
        {
            StopSpawnBatch();
        }
    }
    private void OnDrawGizmosSelected() //dùng để vẽ phạm vi camera padding 
    {
        
        if(mainCamera == null) return;

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
//các code này là vẽ các giới hạn trong game để có thể nhìn được trong game
        Gizmos.DrawWireSphere(new Vector3(left, top, gamePlayPlaneZ),spawnPointGizmoRadius);
        Gizmos.DrawWireSphere(new Vector3(right, top, gamePlayPlaneZ),spawnPointGizmoRadius);
        Gizmos.DrawWireSphere(new Vector3(left, bottom, gamePlayPlaneZ),spawnPointGizmoRadius);
        Gizmos.DrawWireSphere(new Vector3(right, bottom, gamePlayPlaneZ),spawnPointGizmoRadius);

        

    }
}
