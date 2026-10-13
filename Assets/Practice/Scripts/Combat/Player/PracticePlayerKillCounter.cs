using UnityEngine;
using System;

public class PracticePlayerKillCounter : HaiMonoBehaviour
{
    [Header("References")]
    [SerializeField] protected PracticeEnemySpawner enemySpawner;
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
        LoadPlayerHealth();
    }
    protected void LoadEnemySpawner()
    {
        if(enemySpawner != null)//khác null là đã gắn rồi còn null là chưa gắn mới đi tìm gắn vào 
        {
            return;
        }
        enemySpawner = FindFirstObjectByType<PracticeEnemySpawner>();
    }
    protected void LoadPlayerHealth()
    {
        if(playerHealth != null)//khác null là đã gắn rồi còn null là chưa gắn mới đi tìm gắn vào 
        {
            return;
        }
        playerHealth = FindFirstObjectByType<PracticePlayerHealth>();
    }
    protected override void OnEnable()
    {
        base.OnEnable();
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
        if(playerHealth == null) return;
        playerHealth.Died += HandlePlayerDied;
    }
    protected void UnSubscribePlayerEvents()
    {
        if(playerHealth == null) return;
        playerHealth.Died -= HandlePlayerDied;
    }
    protected void SubscribeSpawnerEvents()
    {
        if(enemySpawner == null) return;
        enemySpawner.EnemyDefeated += HandleEnemyDefeated;
    }
    protected void UnSubscribeSpawnerEvents()
    {
        if(enemySpawner == null) return;
        enemySpawner.EnemyDefeated -= HandleEnemyDefeated;
    }
    protected void HandleEnemyDefeated()
    {
        if(!isCounting) return;
        
        currentKills++;
        KillCountChanged?.Invoke(currentKills);
    }
    protected void HandlePlayerDied()
    {
        StopCounting();
    }
    protected void StopCounting()
    {
        if(!isCounting) return;

        isCounting = false;
        KillCountChanged?.Invoke(currentKills);
        KillCountEnded?.Invoke(currentKills);
    }
}
