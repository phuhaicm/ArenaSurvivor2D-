using UnityEngine;

public class EnemySpawner : HaiMonoBehaviour
{
    [SerializeField] private string enemyPrefabPath = GameResourcePaths.BasicEnemy;
    [SerializeField] private float initialDelay = 0.5f;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private int maxAliveEnemies = 5;
    [SerializeField] private float spawnPadding = 0.5f;

    [SerializeField] private EnemyHealth enemyPrefab;
    [SerializeField] private Collider2D spawnBounds;
    [SerializeField] private EnemyContainerRoot enemyContainerRoot;
    [SerializeField] private PlayerHealth playerHealth;

    private float nextSpawnTime;

    protected override void Awake()
    {
        base.Awake();
        nextSpawnTime = Time.time + initialDelay;
    }

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadEnemyPrefab();
        LoadSpawnBounds();
        LoadEnemyContainerRoot();
        LoadPlayerHealth();
    }

    protected override void ResetValues()
    {
        base.ResetValues();
        enemyPrefabPath = GameResourcePaths.BasicEnemy;
        initialDelay = 0.5f;
        spawnInterval = 2f;
        maxAliveEnemies = 5;
        spawnPadding = 0.5f;
    }

    private void Update()
    {
        if (!CanSpawn()) return;

        SpawnEnemy();
        nextSpawnTime = Time.time + spawnInterval;
    }

    private void LoadEnemyPrefab()
    {
        if (enemyPrefab != null) return;
        enemyPrefab = ResourcePrefabLoader.LoadPrefab<EnemyHealth>(enemyPrefabPath);
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

    private bool CanSpawn()
    {
        if (enemyPrefab == null || spawnBounds == null || enemyContainerRoot == null || playerHealth == null) return false;
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
            if (container.GetChild(i).gameObject.activeInHierarchy) count++;
        }

        return count;
    }

    private void SpawnEnemy()
    {
        Vector2 pos = BoundsEdgePositionUtility.RandomPointOnEdge(spawnBounds.bounds, spawnPadding);
        Instantiate(enemyPrefab, pos, Quaternion.identity, enemyContainerRoot.transform);
    }
}