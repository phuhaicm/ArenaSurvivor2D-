using System;
using UnityEngine;

public class PracticeDifficultySystem : HaiMonoBehaviour
{
    [Header("Difficulty Timing")] 
    [SerializeField, Min(1f)] private float difficultyInterval = 30f;
    [Header("Spawn Interval Difficulty")] 
    [SerializeField, Min(0.1f)] private float initialSpawnInterval = 2f;
    [SerializeField, Min(0f)] private float spawnIntervalReduction = 0.2f;// mỗi level giảm 0.2
    [SerializeField, Min(0.05f)] private float minimunSpawnInterval = 0.5f;//giới hạn nhỏ nhất vì cứ giảm mãi thì sẽ bị âm
    [Header("Max Alive Difficulty")] 
    [SerializeField, Min(1f)] private int initialMaxAliveEnemies = 5;
    [SerializeField, Min(0)] private int maxAliveIncreasePerLevel = 2;
    [SerializeField, Min(1)] private int maximunAliveEnemies = 30;
    [Header("Batch Difficulty")] 
    [SerializeField, Min(1f)] private int initialBatchSize = 1;
    [SerializeField, Min(0)] private int maxBatchIncreasePerLevel = 1;
    [SerializeField, Min(1)] private int maximunBatchSize = 5;
    private int currentDifficultyLevel = 1;
    private float nextDifficultyTime;
    public event Action<PracticeDifficultyData> DifficultyChanged;
    public int CurrentDifficultyLevel => currentDifficultyLevel;

    protected override void Start()
    {
        base.Start();
        currentDifficultyLevel = 1;
        nextDifficultyTime = Time.time + difficultyInterval;

        PublishCurrentlyDifficulty();
    }
    private void Update() {
        UpdateDifficultyOverTime();
    }

    private void UpdateDifficultyOverTime()
    {
        bool difficultyInCreased = false;
        while(Time.time >= nextDifficultyTime)
        {
            currentDifficultyLevel++;
            nextDifficultyTime += difficultyInterval;
            difficultyInCreased = true;
        }
        if (difficultyInCreased)
        {
        PublishCurrentlyDifficulty();
        }
    }
    private PracticeDifficultyData CreateCurrentDifficultyData()
    {
        int difficultyIncreaseCount = currentDifficultyLevel - 1;
        float calculatedSpawnInterval = initialSpawnInterval - spawnIntervalReduction * difficultyIncreaseCount;
        float currentSpawnInterval = Mathf.Max(minimunSpawnInterval, calculatedSpawnInterval);

        int calculatedMaxAliveEnemies = initialMaxAliveEnemies + maxAliveIncreasePerLevel * difficultyIncreaseCount;
        int currentMaxAliveEnemis = Mathf.Min(maximunAliveEnemies, calculatedMaxAliveEnemies);

        int calculatedBatchSize = initialBatchSize + maxBatchIncreasePerLevel * difficultyIncreaseCount;
        int currentBatchSize = Mathf.Min(maximunBatchSize, calculatedBatchSize);

        return new PracticeDifficultyData(currentDifficultyLevel, currentSpawnInterval, currentMaxAliveEnemis, currentBatchSize);
    }
    private void PublishCurrentlyDifficulty()
    {
        PracticeDifficultyData difficultyData = CreateCurrentDifficultyData();

        DifficultyChanged?.Invoke(difficultyData);
        Debug.Log(
            $"Difficulty Level: " + $"{difficultyData.Level} | " + $"Spawn Interval: " + $"{difficultyData.SpawnInterval:F2} | "
            + $"Max Alive: " + $"{difficultyData.MaxAliveEnemies}" + $"Batch Size: " + $"{difficultyData.BatchSize}"
        );
    }
    private void OnValidate()
    {
        if(minimunSpawnInterval < initialSpawnInterval)
        {
            Debug.Log(
                $"{name}: Minium Spawn Interval " + "khong nen lon hon Spawn Interval. ", this
            );
        }
        if(maximunAliveEnemies < initialMaxAliveEnemies)
        {
            Debug.Log(
                $"{name}: Maxium Alive Enemies " + "khong nen nho hon initial alive enemies. ", this
            );
        }
    }
}
