using System;
using UnityEngine;

public class PracticeRunScore : HaiMonoBehaviour
{
    [Header("Reference")]
    [SerializeField] protected PracticePlayerHealth playerHealth;
    [SerializeField] protected PracticeSurvivalTimer survivalTimer;
    [SerializeField] protected PracticePlayerKillCounter playerKillCounter;

    [Header("Score Settings")]
    [SerializeField, Min(0)] protected int pointPerSeconds = 10;
    [SerializeField, Min(0)] protected int pointPerKills = 100;
    public event Action<int> ScoreCalculated;
    protected int finalScore;
    protected bool isScore;
    public int FinalScore => finalScore;
    public bool IsScore => isScore;
    protected override void LoadComponents()
    {
        base.LoadComponents();
        
        LoadPlayerHealth();
        LoadSurvivalTimer();
        LoadPlayerKillCounter();
    }
    protected void LoadPlayerHealth()
    {
        if(playerHealth != null) return;

        playerHealth = FindFirstObjectByType<PracticePlayerHealth>();
    }
    protected void LoadSurvivalTimer()
    {
        if(survivalTimer != null) return;

        survivalTimer = FindFirstObjectByType<PracticeSurvivalTimer>();
    }
    protected void LoadPlayerKillCounter()
    {
        if(playerKillCounter != null) return;

        playerKillCounter = FindFirstObjectByType<PracticePlayerKillCounter>();
    }
    protected override void Start()
    {
        base.Start();
        

        finalScore = 0;
        isScore = false;
        ScoreCalculated?.Invoke(finalScore);
    }
    protected override void OnEnable()
{
    base.OnEnable();

    Debug.Log(
        $"{name}: OnEnable | " +
        $"PlayerHealth = {playerHealth}",
        this
    );

    LoadPlayerHealth();

    Debug.Log(
        $"{name}: Sau LoadPlayerHealth | " +
        $"PlayerHealth = {playerHealth}",
        this
    );

    SubscribePlayerEvents();
}

    protected override void OnDisable()
    {
        UnSubscribePlayerEvents();
        
        base.OnDisable();
    }
    protected void SubscribePlayerEvents()
    {
        if(playerHealth == null) return;
        playerHealth.Died += HandlePlayerDied;
    }

    protected void UnSubscribePlayerEvents()
    {
        if(playerHealth == null) return;
        playerHealth.Died -= HandlePlayerDied;
    }
    protected void HandlePlayerDied()
    {
        CalcutedScore();
    }
    protected void CalcutedScore()
    {
        if(isScore) return;
        if(survivalTimer == null) return;
        if(playerKillCounter == null) return;

        int survivalSecond = Mathf.FloorToInt(survivalTimer.ElapsedTime);
        int currentKills = playerKillCounter.CurrentKills;
        int timeScore = survivalSecond * pointPerSeconds;
        int killScore = currentKills * pointPerKills;
        finalScore = timeScore * killScore;

        isScore = true;
        ScoreCalculated?.Invoke(finalScore);
    }



}
