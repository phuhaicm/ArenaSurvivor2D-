using System;
using UnityEngine;

public class PracticeDifficultySystem : HaiMonoBehaviour
{
    [Header("Difficulty Timing")]
    [SerializeField, Min(1f)] private float difficultyInterval = 25f;

    [Header("Spawn Interval Difficulty")]
    [SerializeField, Min(0.1f)] private float initialSpawnInterval = 1.6f;
    [SerializeField, Min(0f)] private float spawnIntervalReduction = 0.15f;
    [SerializeField, Min(0.05f)] private float minimunSpawnInterval = 0.35f;

    [Header("Max Alive Difficulty")]
    [SerializeField, Min(1)] private int initialMaxAliveEnemies = 10;
    [SerializeField, Min(0)] private int maxAliveIncreasePerLevel = 6;
    [SerializeField, Min(1)] private int maximunAliveEnemies = 80;

    [Header("Batch Difficulty")]
    [SerializeField, Min(1)] private int initialBatchSize = 2;
    [SerializeField, Min(0)] private int maxBatchIncreasePerLevel = 1;
    [SerializeField, Min(1)] private int maximunBatchSize = 8;

    private int currentDifficultyLevel = 1;
    private float nextDifficultyTime;
    private bool isBenchmarkOverrideActive;

    public event Action<PracticeDifficultyData> DifficultyChanged;
    public int CurrentDifficultyLevel => currentDifficultyLevel;
    public bool IsBenchmarkOverrideActive => isBenchmarkOverrideActive;

    protected override void Start()
    {
        base.Start();

        if (isBenchmarkOverrideActive)
        {
            return;
        }

        currentDifficultyLevel = 1;
        nextDifficultyTime = Time.time + difficultyInterval;

        PublishCurrentlyDifficulty();
    }

    private void Update()
    {
        if (isBenchmarkOverrideActive)
        {
            return;
        }

        UpdateDifficultyOverTime();
    }

    private void UpdateDifficultyOverTime()
    {
        bool difficultyIncreased = false;

        while (Time.time >= nextDifficultyTime)
        {
            currentDifficultyLevel++;
            nextDifficultyTime += difficultyInterval;
            difficultyIncreased = true;
        }

        if (difficultyIncreased)
        {
            PublishCurrentlyDifficulty();
        }
    }

    private PracticeDifficultyData CreateCurrentDifficultyData()
    {
        int difficultyIncreaseCount = currentDifficultyLevel - 1;
        float calculatedSpawnInterval =
            initialSpawnInterval - spawnIntervalReduction * difficultyIncreaseCount;
        float currentSpawnInterval =
            Mathf.Max(minimunSpawnInterval, calculatedSpawnInterval);

        int calculatedMaxAliveEnemies =
            initialMaxAliveEnemies + maxAliveIncreasePerLevel * difficultyIncreaseCount;
        int currentMaxAliveEnemies =
            Mathf.Min(maximunAliveEnemies, calculatedMaxAliveEnemies);

        int calculatedBatchSize =
            initialBatchSize + maxBatchIncreasePerLevel * difficultyIncreaseCount;
        int currentBatchSize =
            Mathf.Min(maximunBatchSize, calculatedBatchSize);

        return new PracticeDifficultyData(
            currentDifficultyLevel,
            currentSpawnInterval,
            currentMaxAliveEnemies,
            currentBatchSize
        );
    }

    private void PublishCurrentlyDifficulty()
    {
        PracticeDifficultyData difficultyData = CreateCurrentDifficultyData();

        DifficultyChanged?.Invoke(difficultyData);
        Debug.Log(
            $"Difficulty Level: {difficultyData.Level} | " +
            $"Spawn Interval: {difficultyData.SpawnInterval:F2} | " +
            $"Max Alive: {difficultyData.MaxAliveEnemies} | " +
            $"Batch Size: {difficultyData.BatchSize}"
        );
    }

    public void ApplyCustomBenchmarkDifficulty(
        int targetLevel,
        float customSpawnInterval,
        int customMaxAliveEnemies,
        int customBatchSize
    )
    {
        isBenchmarkOverrideActive = true;
        currentDifficultyLevel = Mathf.Max(1, targetLevel);

        PracticeDifficultyData customData = new PracticeDifficultyData(
            currentDifficultyLevel,
            Mathf.Max(0.02f, customSpawnInterval),
            Mathf.Max(1, customMaxAliveEnemies),
            Mathf.Max(1, customBatchSize)
        );

        DifficultyChanged?.Invoke(customData);
    }

    public void DisableBenchmarkOverride()
    {
        isBenchmarkOverrideActive = false;
        nextDifficultyTime = Time.time + difficultyInterval;
        PublishCurrentlyDifficulty();
    }

    private void OnValidate()
    {
        if (minimunSpawnInterval > initialSpawnInterval)
        {
            Debug.LogWarning(
                $"{name}: Minimum Spawn Interval không nên lớn hơn Initial Spawn Interval.",
                this
            );
        }

        if (maximunAliveEnemies < initialMaxAliveEnemies)
        {
            Debug.LogWarning(
                $"{name}: Maximum Alive Enemies không nên nhỏ hơn Initial Alive Enemies.",
                this
            );
        }
    }
}