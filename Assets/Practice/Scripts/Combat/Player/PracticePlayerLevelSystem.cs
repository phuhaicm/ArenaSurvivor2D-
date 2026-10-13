using System;
using UnityEngine;

public class PracticePlayerLevelSystem : HaiMonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField, Min(1)] protected int startingLevel = 1;
    [SerializeField, Min(1f)] protected float startingExperienceRequired = 10f;
    [SerializeField, Min(1f)] protected float experienceMultiplier = 5f;
    [SerializeField] PracticePlayerExperience playerExperience;
    [SerializeField] protected float currentExperience;
    [SerializeField] protected float currentLevel;
    [SerializeField] protected float experienceToNextLevel;
    public event Action<float, float> ExperienceChanged;
    public event Action<float> LevelChanged; // event thông báo lên cấp    
    public float CurrenLevel => currentLevel;
    public float CurrentExperience => currentExperience;
    public float ExperienceToNextLevel => experienceToNextLevel;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadPlayerExperience();
    }
    protected virtual void LoadPlayerExperience()
    {
        if(playerExperience != null) return;

        playerExperience = GetComponent<PracticePlayerExperience>();
    }
    protected override void Awake()
    {
        base.Awake();
        currentLevel = startingLevel;
        currentExperience = 0f;
        experienceToNextLevel = startingExperienceRequired;
    }
    protected override void Start()
    {
        base.Start();

        PublishCurrentProgress();
    }
    protected override void OnEnable()
    {
        base.OnEnable();

        RegisterEventLevel();
    }
    protected override void OnDisable()
    {
        base.OnDisable();

        UnRegisterEventLevel();
    }
    protected virtual void RegisterEventLevel()
    {
        if(playerExperience == null) return;

        playerExperience.ExperienceGained += HandleExperienceGained;
    }
    protected virtual void UnRegisterEventLevel()
    {
        if(playerExperience == null) return;

        playerExperience.ExperienceGained -= HandleExperienceGained;
    }

    protected virtual void HandleExperienceGained(float amount)
    {
        if (amount <= 0f) return;

        currentExperience += amount;
        
        CheckLevelUp();
        PublishCurrentProgress();

        Debug.Log($"{name} Level: {currentLevel} | " + 
        $"XP: {currentExperience:F1}/" + $"{experienceToNextLevel:F1}", gameObject);
    }
    protected virtual void CheckLevelUp()
    {
        while(currentExperience >= experienceToNextLevel)
        {
            currentExperience -= experienceToNextLevel;
            LevelUp();
        }
    }
    protected virtual void LevelUp()
    {
        currentLevel++;
        experienceToNextLevel += experienceMultiplier;

        Debug.Log($"Level Up! Current Level: " + $"{currentLevel}", gameObject);// gameOjbect là trỏ toàn bộ player còn this là chỉ riêng 1 componet thôi log này muốn thông báo cho player nên dùng gameObject
        LevelChanged?.Invoke(currentLevel);//phát tbao player vừa lên cấp 
    }
    protected virtual void PublishCurrentProgress()
    {
        ExperienceChanged?.Invoke(currentExperience, experienceToNextLevel);
    }
    protected override void ResetValues()
    {
        base.ResetValues();
        startingLevel = 1;
        startingExperienceRequired = 5f;
        experienceToNextLevel = 5f;

    }
}
