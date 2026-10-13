using UnityEngine;

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Practice/Upgrade Definition")]
public class PracticeUpgradeDefinition : ScriptableObject {
    [Header("Display")]
    [SerializeField] private string displayName = "New Upgrade";
    [SerializeField, TextArea] private string description = "Upgrade description";
    [Header("Effect")]
    [SerializeField] PracticeUpgradeType upgradeType;
    [SerializeField, Min(0f)] private float baseValue = 1f;
    [SerializeField, Min(0f)] private float valueIncreasePerLevel;
    [SerializeField, Min(1f)] private int maxLevel = 5;
    public string DisplayName => displayName;
    public string Description => description;
    public PracticeUpgradeType UpgradeType => upgradeType;
    public float BaseValue => baseValue;
    public float ValueIncreasePerLevel => valueIncreasePerLevel;
    public int MaxLevel => maxLevel;
    public float GetValuePerLevel(int level)
    {
        int validLevel = Mathf.Max(1, level);

        return baseValue + valueIncreasePerLevel * (validLevel - 1);
    }

}
