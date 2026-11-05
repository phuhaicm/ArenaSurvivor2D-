using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct PracticeDifficultyData 
{
    public int Level { get; }
    public float SpawnInterval { get; }
    public int MaxAliveEnemies { get; }
    public int BatchSize { get; }
    public PracticeDifficultyData(int level, float spawnInterval, int maxAliveEnemies, int batchSize)
    {
        Level = level;
        SpawnInterval = spawnInterval;
        MaxAliveEnemies = maxAliveEnemies;
        BatchSize = batchSize;
    }
}
