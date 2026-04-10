using UnityEngine;

public class EnemySpawner : HaiMonoBehaviour
{
    [SerializeField] private float initialDelay = 0.5f;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private int maxAliveEnemies = 5;
    [SerializeField] private float spawnPadding = 0.5f;

    [SerializeField] private Collider2D spawnBounds;
    [SerializeField] private EnemyContainerRoot enemyContainerRoot;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private SurvivalTimer survivalTimer;

    private float nextSpawnTime;

    protected override void Awake()
    {
        base.Awake();
        nextSpawnTime = Time.time + initialDelay;
    }

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadSpawnBounds();
        LoadEnemyContainerRoot();
        LoadPlayerHealth();
        LoadSurvivalTimer();
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
        if (!CanSpawn()) return;

        SpawnEnemyVariant();
        nextSpawnTime = Time.time + spawnInterval;
    }

    private void LoadSpawnBounds()
    {
        if (spawnBounds != null) return;

        PlayAreaBoundsMarker marker = FindFirstObjectByType<PlayAreaBoundsMarker>();
        if (marker == null) return;

        spawnBounds = marker.GetComponent<Collider2D>();
    }

    private void LoadEnemyContainerRoot()
    {
        if (enemyContainerRoot != null) return;
        enemyContainerRoot = FindFirstObjectByType<EnemyContainerRoot>();
    }

    private void LoadPlayerHealth()
    {
        if (playerHealth != null) return;
        playerHealth = FindFirstObjectByType<PlayerHealth>();
    }

    private void LoadSurvivalTimer()
    {
        if (survivalTimer != null) return;
        survivalTimer = FindFirstObjectByType<SurvivalTimer>();
    }

    private bool CanSpawn()
    {
        if (spawnBounds == null || enemyContainerRoot == null || playerHealth == null || survivalTimer == null) return false;
        if (playerHealth.IsDead) return false;
        if (Time.time < nextSpawnTime) return false;
        if (CountAliveEnemies() >= maxAliveEnemies) return false;
        return true;
    }

    private int CountAliveEnemies()
    {
        int count = 0;
        Transform container = enemyContainerRoot.transform;

        for (int i = 0; i < container.childCount; i++)
        {
            if (container.GetChild(i).gameObject.activeInHierarchy)
            {
                count++;
            }
        }

        return count;
    }

    private void SpawnEnemyVariant()
    {
        float survivalTime = survivalTimer.ElapsedTime;
        EnemyVariantType variantType = EnemyVariantCatalog.GetRandomVariant(survivalTime);
        string resourcePath = EnemyVariantCatalog.GetResourcePath(variantType);

        EnemyHealth prefab = ResourcePrefabLoader.LoadPrefab<EnemyHealth>(resourcePath);
        if (prefab == null) return;

        Vector2 spawnPosition = BoundsEdgePositionUtility.RandomPointOnEdge(spawnBounds.bounds, spawnPadding);
        Instantiate(prefab, spawnPosition, Quaternion.identity, enemyContainerRoot.transform);
    }

    public void SetSpawnInterval(float value)
    {
        spawnInterval = Mathf.Max(0.1f, value);
    }

    public void SetMaxAliveEnemies(int value)
    {
        maxAliveEnemies = Mathf.Max(1, value);
    }
}