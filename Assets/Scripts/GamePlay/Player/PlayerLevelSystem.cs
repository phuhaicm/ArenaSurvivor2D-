using System;
using UnityEngine;

public class PlayerLevelSystem : HaiMonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentLevelExperience = 0;
    [SerializeField] private int requiredExperience = 10;

    private PlayerExperience playerExperience;

    public int CurrentLevel => currentLevel;
    public int CurrentLevelExperience => currentLevelExperience;
    public int RequiredExperience => requiredExperience;
    public float ProgressNormalized => requiredExperience <= 0 ? 0f : (float)currentLevelExperience / requiredExperience;

    public event Action<int> LeveledUp;
    public event Action LevelStateChanged;

    protected override void Awake()
    {
        base.Awake();
        InitializeLevelState();
    }

    protected override void Start()
    {
        base.Start();
        NotifyLevelStateChanged();
    }

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadPlayerExperience();
    }

    protected override void ResetValues()
    {
        base.ResetValues();
        ResetLevelDefaults();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SubscribeEvents();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        UnsubscribeEvents();
    }

    private void LoadPlayerExperience()
    {
        if (playerExperience != null) return;
        playerExperience = GetComponent<PlayerExperience>();
    }

    private void ResetLevelDefaults()
    {
        currentLevel = 1;
        currentLevelExperience = 0;
        requiredExperience = ExperienceProgressionCalculator.GetRequiredExperienceForLevel(currentLevel);
    }

    private void InitializeLevelState()
    {
        currentLevel = Mathf.Max(1, currentLevel);
        requiredExperience = Mathf.Max(1, requiredExperience);
        currentLevelExperience = Mathf.Clamp(currentLevelExperience, 0, requiredExperience);
    }

    private void SubscribeEvents()
    {
        if (playerExperience != null)
        {
            playerExperience.ExperienceGained += HandleExperienceGained;
        }
    }

    private void UnsubscribeEvents()
    {
        if (playerExperience != null)
        {
            playerExperience.ExperienceGained -= HandleExperienceGained;
        }
    }

    private void HandleExperienceGained(int amount)
    {
        if (amount <= 0) return;

        currentLevelExperience += amount;
        ProcessPendingLevelUps();
        NotifyLevelStateChanged();
    }

    private void ProcessPendingLevelUps()
    {
        while (currentLevelExperience >= requiredExperience)
        {
            currentLevelExperience -= requiredExperience;
            LevelUpOnce();
        }
    }

    private void LevelUpOnce()
    {
        currentLevel++;
        requiredExperience = ExperienceProgressionCalculator.GetRequiredExperienceForLevel(currentLevel);

        LeveledUp?.Invoke(currentLevel);
        NotifyLevelStateChanged();
    }

    private void NotifyLevelStateChanged()
    {
        LevelStateChanged?.Invoke();
    }
}