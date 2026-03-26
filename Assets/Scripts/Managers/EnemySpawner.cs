using UnityEngine;

public class EnemySpawner : HaiMonoBehaviour
{
    private static EnemySpawner instance;
    public static EnemySpawner Instance => instance;

    [Header("Spawn Settings")]
    [SerializeField] private EnemyHealth enemyPrefab;
    [SerializeField] private float initialDelay = 0.5f;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private int maxAliveEnemies = 5;
    [SerializeField] private float spawnPadding = 0.5f;

    [Header("Scene References")]
    [SerializeField] private Collider2D spawnBounds;
    [SerializeField] private EnemyContainerRoot enemyContainerRoot;
    [SerializeField] private PlayerHealth playerHealth;

    private float nextSpawnTime;

    protected override void Awake()
    {
        if (!TrySetInstance())
        {
            return;
        }

        base.Awake();
        InitializeSpawner();
    }

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadSpawnBounds();
        LoadEnemyContainerRoot();
        LoadPlayerHealth();
    }

    protected override void ResetValues()
    {
        base.ResetValues();

        initialDelay = 0.5f;
        spawnInterval = 2f;
        maxAliveEnemies = 5;
        spawnPadding = 0.5f;
    }

    private void Update()
    {
        TrySpawnOnInterval();
    }

    private bool TrySetInstance()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("Only one EnemySpawner is allowed to exist.", gameObject);
            return false;
        }

        instance = this;
        return true;
    }

    private void InitializeSpawner()
    {
        nextSpawnTime = Time.time + initialDelay;
    }

    protected virtual void LoadSpawnBounds()
    {
        if (spawnBounds != null) return;

        PlayAreaBoundsMarker boundsMarker = FindFirstObjectByType<PlayAreaBoundsMarker>();

        if (boundsMarker == null)
        {
            Debug.LogWarning($"{name}: PlayAreaBoundsMarker not found.", gameObject);
            return;
        }

        spawnBounds = boundsMarker.GetComponent<Collider2D>();

        if (spawnBounds == null)
        {
            Debug.LogError($"{name}: PlayAreaBoundsMarker is missing Collider2D.", boundsMarker.gameObject);
            return;
        }

        LogLoad(nameof(LoadSpawnBounds));
    }

    protected virtual void LoadEnemyContainerRoot()
    {
        if (enemyContainerRoot != null) return;

        enemyContainerRoot = FindFirstObjectByType<EnemyContainerRoot>();

        if (enemyContainerRoot == null)
        {
            Debug.LogWarning($"{name}: EnemyContainerRoot not found.", gameObject);
            return;
        }

        LogLoad(nameof(LoadEnemyContainerRoot));
    }

    protected virtual void LoadPlayerHealth()
    {
        if (playerHealth != null) return;

        playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogWarning($"{name}: PlayerHealth not found.", gameObject);
            return;
        }

        LogLoad(nameof(LoadPlayerHealth));
    }

    private void TrySpawnOnInterval()
    {
        if (!CanSpawn())
        {
            return;
        }

        SpawnEnemy();
        ScheduleNextSpawn();
    }

    private bool CanSpawn()
    {
        if (enemyPrefab == null)
        {
            return false;
        }

        if (spawnBounds == null || enemyContainerRoot == null || playerHealth == null)
        {
            return false;
        }

        if (playerHealth.IsDead)
        {
            return false;
        }

        if (Time.time < nextSpawnTime)
        {
            return false;
        }

        if (CountAliveEnemies() >= maxAliveEnemies)
        {
            return false;
        }

        return true;
    }

    private void ScheduleNextSpawn()
    {
        nextSpawnTime = Time.time + spawnInterval;
    }

    private int CountAliveEnemies()
    {
        if (enemyContainerRoot == null)
        {
            return 0;
        }

        int aliveCount = 0;
        Transform containerTransform = enemyContainerRoot.transform;

        for (int i = 0; i < containerTransform.childCount; i++)
        {
            Transform child = containerTransform.GetChild(i);

            if (child.gameObject.activeInHierarchy)
            {
                aliveCount++;
            }
        }

        return aliveCount;
    }

    private Vector2 GetSpawnPositionOnBounds()
    {
        Bounds bounds = spawnBounds.bounds;

        float minX = bounds.min.x + spawnPadding;
        float maxX = bounds.max.x - spawnPadding;
        float minY = bounds.min.y + spawnPadding;
        float maxY = bounds.max.y - spawnPadding;

        int side = Random.Range(0, 4);

        switch (side)
        {
            case 0:
                return new Vector2(Random.Range(minX, maxX), maxY);

            case 1:
                return new Vector2(Random.Range(minX, maxX), minY);

            case 2:
                return new Vector2(minX, Random.Range(minY, maxY));

            default:
                return new Vector2(maxX, Random.Range(minY, maxY));
        }
    }

    private void SpawnEnemy()
    {
        Vector2 spawnPosition = GetSpawnPositionOnBounds();

        EnemyHealth spawnedEnemy = Instantiate(
            enemyPrefab,
            spawnPosition,
            Quaternion.identity,
            enemyContainerRoot.transform
        );

        Debug.Log($"{name}: Spawned enemy at {spawnPosition}", gameObject);
    }
}