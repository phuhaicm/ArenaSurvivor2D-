using System;
using UnityEngine;

public class PracticeFinalBossController : HaiMonoBehaviour
{
    [Header("Boss Setup")]
    [SerializeField] protected PracticeEnemyHealth bossPrefab;
    [SerializeField] protected Transform bossSpawnPoint;
    [SerializeField] protected Transform bossContainer;
    [SerializeField] protected PracticeEnemyHealth activeBoss;
    protected bool bossSpawned;
    protected bool bossDefeated;
    public event Action BossSpawned;
    public event Action BossDefeated;
    public bool HasActiveBoss => activeBoss != null && !activeBoss.IsDead;
    public bool IsBossDefeated => bossDefeated;
    public PracticeEnemyHealth ActiveBoss => activeBoss;

    protected override void OnDisable()
    {
        UnsubscribeActiveBoss();

        base.OnDisable();
    }
    public bool SpawnBoss()
    {
        if(bossSpawned) return false;

        if(bossPrefab == null)
        {
            Debug.LogError($"{name}: Boss Prefab null.", this);
            return false;
        }

        if(bossSpawnPoint == null)
        {
            Debug.LogError($"{name}: Boss Spawn Point null. ", this);
            return false;
        }
        bossSpawned = true;
        bossDefeated = false;

        activeBoss = Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation, bossContainer);

        if(activeBoss == null)
        {
            bossSpawned = false;

            Debug.LogError($"{name}: Spawn Boss thất bại.", this);
            return false;
        }

        SubscribeActiveBoss();
        BossSpawned?.Invoke();
        return true;
    }
    protected void SubscribeActiveBoss()
    {
        if(activeBoss == null) return;

        activeBoss.Died += HandleBossDied;
    }
    protected void UnsubscribeActiveBoss()
    {
        if(activeBoss == null) return;

        activeBoss.Died -= HandleBossDied;
    }
    protected void HandleBossDied()
    {
        UnsubscribeActiveBoss();

        bossDefeated = true;
        BossDefeated?.Invoke();
        activeBoss = null;
    }
}
