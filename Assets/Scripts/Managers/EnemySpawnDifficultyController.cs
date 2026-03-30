using UnityEngine;

public class EnemySpawnDifficultyController : HaiMonoBehaviour
{
    private EnemySpawner enemySpawner;
    private SurvivalTimer survivalTimer;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadEnemySpawner();
        LoadSurvivalTimer();
    }

    private void LoadEnemySpawner()
    {
        if (enemySpawner != null) return;
        enemySpawner = FindFirstObjectByType<EnemySpawner>();
    }

    private void LoadSurvivalTimer()
    {
        if (survivalTimer != null) return;
        survivalTimer = FindFirstObjectByType<SurvivalTimer>();
    }

    private void Update()
    {
        if (enemySpawner == null || survivalTimer == null) return;

        float survivalTime = survivalTimer.ElapsedTime;

        enemySpawner.SetSpawnInterval(SpawnDifficultyCurve.GetSpawnInterval(survivalTime));
        enemySpawner.SetMaxAliveEnemies(SpawnDifficultyCurve.GetMaxAliveEnemies(survivalTime));
    }
}
