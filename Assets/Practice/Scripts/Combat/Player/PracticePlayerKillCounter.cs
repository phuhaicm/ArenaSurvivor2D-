using System;
using UnityEngine;

public class PracticePlayerKillCounter : HaiMonoBehaviour
{
    [Header("References")]
    [SerializeField] protected PracticeEnemySpawner enemySpawner;
    [SerializeField] protected PracticeFinalBossController bossController;
    [SerializeField] protected PracticePlayerHealth playerHealth;
    [SerializeField] private int currentKills;
    [SerializeField] private bool isCounting;

    public event Action<int> KillCountChanged;
    public event Action<int> KillCountEnded;

    public int CurrentKills => currentKills;
    public bool IsCounting => isCounting;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadEnemySpawner();
        LoadBossController();
        LoadPlayerHealth();
    }

    protected void LoadEnemySpawner()
    {
        if (enemySpawner != null)
        {
            return;
        }
        enemySpawner = FindFirstObjectByType<PracticeEnemySpawner>();
    }

    protected void LoadBossController()
    {
        if (bossController != null)
        {
            return;
        }
        bossController = FindFirstObjectByType<PracticeFinalBossController>();
    }

    protected void LoadPlayerHealth()
    {
        if (playerHealth != null)
        {
            return;
        }
        playerHealth = FindFirstObjectByType<PracticePlayerHealth>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        LoadEnemySpawner();
        LoadBossController();
        LoadPlayerHealth();
        SubscribeSpawnerEvents();
        SubscribePlayerEvents();
    }

    protected override void OnDisable()
    {
        UnSubscribeSpawnerEvents();
        UnSubscribePlayerEvents();
        base.OnDisable();
    }

    protected override void Start()
    {
        base.Start();
        StartCounting();
    }

    protected void StartCounting()
    {
        currentKills = 0;
        isCounting = true;

        KillCountChanged?.Invoke(currentKills);
    }

    protected void SubscribePlayerEvents()
    {
        if (playerHealth == null) return;
        playerHealth.Died -= HandlePlayerDied;
        playerHealth.Died += HandlePlayerDied;
    }

    protected void UnSubscribePlayerEvents()
    {
        if (playerHealth == null) return;
        playerHealth.Died -= HandlePlayerDied;
    }

    protected void SubscribeSpawnerEvents()
    {
        if (enemySpawner != null)
        {
            enemySpawner.EnemyDefeated -= HandleEnemyDefeated;
            enemySpawner.EnemyDefeated += HandleEnemyDefeated;
        }

        if (bossController != null)
        {
            bossController.BossDefeated -= HandleEnemyDefeated;
            bossController.BossDefeated += HandleEnemyDefeated;
        }
    }

    protected void UnSubscribeSpawnerEvents()
    {
        if (enemySpawner != null)
        {
            enemySpawner.EnemyDefeated -= HandleEnemyDefeated;
        }

        if (bossController != null)
        {
            bossController.BossDefeated -= HandleEnemyDefeated;
        }
    }

    protected void HandleEnemyDefeated()
    {
        if (!isCounting) return;

        currentKills++;
        KillCountChanged?.Invoke(currentKills);
    }

    protected void HandlePlayerDied()
    {
        StopCounting();
    }

    protected void StopCounting()
    {
        if (!isCounting) return;

        isCounting = false;
        KillCountChanged?.Invoke(currentKills);
        KillCountEnded?.Invoke(currentKills);
    }
}