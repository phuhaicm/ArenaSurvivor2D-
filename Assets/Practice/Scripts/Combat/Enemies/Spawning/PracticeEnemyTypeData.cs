using UnityEngine;

[CreateAssetMenu(
    fileName = "NewEnemyType",
    menuName = "Practice/Enemy Type"
)]

public class PracticeEnemyTypeData : ScriptableObject {
    [Header("Identity")]
    [SerializeField] private string enemyTypeName = "New Enemy Type";
    [Header("Prefab")]
    [SerializeField] private PracticeEnemyHealth enemyPrefab;//prefab tương ứng với loại enemy như fast tank normal
    [Header("Unlock Requirement")]
    [SerializeField, Min(1)] private int unlockDifficultyLevel = 1;
    [Header("Weight")]
    [SerializeField, Min(0f)] private float spawnWeight = 1f;
    public string EnemyTypeName => enemyTypeName;
    public PracticeEnemyHealth EnemyPrefab => enemyPrefab;
    public int UnlockDifficultyLevel => unlockDifficultyLevel;
    public float SpawnWeight => spawnWeight;
    
}
