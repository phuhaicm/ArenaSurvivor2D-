using UnityEngine;

public static class SpawnDifficultyCurve
{
    public static float GetSpawnInterval(float survivalTime)
    {
        if (survivalTime < 30f) return 2f;
        if (survivalTime < 60f) return 1.6f;
        if (survivalTime < 90f) return 1.2f;
        return 0.9f;
    }

    public static int GetMaxAliveEnemies(float survivalTime)
    {
        if (survivalTime < 30f) return 5;
        if (survivalTime < 60f) return 7;
        if (survivalTime < 90f) return 10;
        return 14;
    }
}