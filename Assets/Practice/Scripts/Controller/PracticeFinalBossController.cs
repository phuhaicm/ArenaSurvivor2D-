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
    private string currentBossTitle = "FINAL BOSS";

    public event Action BossSpawned;
    public event Action BossDefeated;

    public bool HasActiveBoss => activeBoss != null && !activeBoss.IsDead;
    public bool IsBossDefeated => bossDefeated;
    public PracticeEnemyHealth ActiveBoss => activeBoss;
    public string CurrentBossTitle => currentBossTitle;

    protected override void OnDisable()
    {
        UnsubscribeActiveBoss();
        base.OnDisable();
    }

    // Phương thức spawn linh hoạt cho từng Đợt (Wave 1 Mid-Boss vs Wave 2 Final Boss)
    public bool SpawnBossEncounter(
        string bossTitle = "FINAL BOSS",
        float healthMultiplier = 1f,
        int allowedMaxPhaseIndex = -1
    )
    {
        if (bossSpawned && HasActiveBoss)
        {
            return false;
        }

        if (bossPrefab == null)
        {
            Debug.LogError($"{name}: Boss Prefab null.", this);
            return false;
        }

        if (bossSpawnPoint == null)
        {
            Debug.LogError($"{name}: Boss Spawn Point null.", this);
            return false;
        }

        bossSpawned = true;
        bossDefeated = false;
        currentBossTitle = string.IsNullOrWhiteSpace(bossTitle) ? "FINAL BOSS" : bossTitle;

        activeBoss = Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation, bossContainer);

        if (activeBoss == null)
        {
            bossSpawned = false;
            Debug.LogError($"{name}: Spawn Boss thất bại.", this);
            return false;
        }

        // Cấu hình số cây máu và hệ số máu riêng cho từng đợt Boss
        PracticeBossAttackController attackController =
            activeBoss.GetComponent<PracticeBossAttackController>();

        if (attackController != null)
        {
            attackController.ConfigureEncounterRules(healthMultiplier, allowedMaxPhaseIndex);
        }

        SubscribeActiveBoss();
        BossSpawned?.Invoke();
        return true;
    }

    public bool SpawnBoss()
    {
        return SpawnBossEncounter("FINAL BOSS", 1f, -1);
    }

    protected void SubscribeActiveBoss()
    {
        if (activeBoss == null) return;
        activeBoss.Died += HandleBossDied;
    }

    protected void UnsubscribeActiveBoss()
    {
        if (activeBoss == null) return;
        activeBoss.Died -= HandleBossDied;
    }

    protected void HandleBossDied()
    {
        UnsubscribeActiveBoss();

        bossDefeated = true;
        bossSpawned = false; // Reset cờ để đợt sóng sau có thể triệu hồi Boss tiếp theo!
        BossDefeated?.Invoke();
        activeBoss = null;
    }
}
