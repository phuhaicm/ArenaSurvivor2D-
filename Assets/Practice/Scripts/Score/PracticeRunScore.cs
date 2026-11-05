using System;
using UnityEngine;

public class PracticeRunScore : HaiMonoBehaviour
{
    [Header("Reference")]
    [SerializeField] protected PracticePlayerHealth playerHealth;
    [SerializeField] protected PracticeSurvivalTimer survivalTimer;
    [SerializeField] protected PracticePlayerKillCounter playerKillCounter;
    [SerializeField] protected PracticeGameStateController gameStateController;
    [SerializeField] protected PracticePlayerLevelSystem playerLevelSystem;

    [Header("Score Settings")]
    [SerializeField, Min(0)] protected int pointPerSeconds = 10;
    [SerializeField, Min(0)] protected int pointPerKills = 100;

    public event Action<int> ScoreCalculated;

    protected int finalScore;
    protected bool isScore;
    private int lastRecordedSecond = -1;

    public int FinalScore => finalScore;
    public bool IsScore => isScore;

    protected override void LoadComponents()
    {
        base.LoadComponents();

        LoadPlayerHealth();
        LoadSurvivalTimer();
        LoadPlayerKillCounter();
        LoadGameStateController();
        LoadPlayerLevelSystem();
    }

    protected void LoadPlayerHealth()
    {
        if (playerHealth != null) return;
        playerHealth = FindFirstObjectByType<PracticePlayerHealth>();
    }

    protected void LoadSurvivalTimer()
    {
        if (survivalTimer != null) return;
        survivalTimer = FindFirstObjectByType<PracticeSurvivalTimer>();
    }

    protected void LoadPlayerKillCounter()
    {
        if (playerKillCounter != null) return;
        playerKillCounter = FindFirstObjectByType<PracticePlayerKillCounter>();
    }

    protected void LoadGameStateController()
    {
        if (gameStateController != null) return;
        gameStateController = FindFirstObjectByType<PracticeGameStateController>();
    }

    protected void LoadPlayerLevelSystem()
    {
        if (playerHealth != null)
        {
            PracticePlayerLevelSystem playerLevel =
                playerHealth.GetComponentInChildren<PracticePlayerLevelSystem>(true);
            if (playerLevel != null)
            {
                playerLevelSystem = playerLevel;
                return;
            }
        }

        if (playerLevelSystem != null) return;
        playerLevelSystem = FindFirstObjectByType<PracticePlayerLevelSystem>();
    }

    protected override void Start()
    {
        base.Start();

        finalScore = 0;
        isScore = false;
        lastRecordedSecond = -1;
        ScoreCalculated?.Invoke(finalScore);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        LoadPlayerHealth();
        LoadSurvivalTimer();
        LoadPlayerKillCounter();
        LoadGameStateController();
        LoadPlayerLevelSystem();

        SubscribeEvents();
    }

    protected override void OnDisable()
    {
        UnSubscribeEvents();

        base.OnDisable();
    }

    protected void SubscribeEvents()
    {
        if (playerHealth != null)
        {
            playerHealth.Died -= HandlePlayerDied;
            playerHealth.Died += HandlePlayerDied;
        }

        if (gameStateController != null)
        {
            gameStateController.StateChanged -= HandleGameStateChanged;
            gameStateController.StateChanged += HandleGameStateChanged;
        }

        if (survivalTimer != null)
        {
            survivalTimer.TimeChanged -= HandleTimeChanged;
            survivalTimer.TimeChanged += HandleTimeChanged;
        }

        if (playerKillCounter != null)
        {
            playerKillCounter.KillCountChanged -= HandleKillCountChanged;
            playerKillCounter.KillCountChanged += HandleKillCountChanged;
        }
    }

    protected void UnSubscribeEvents()
    {
        if (playerHealth != null)
        {
            playerHealth.Died -= HandlePlayerDied;
        }

        if (gameStateController != null)
        {
            gameStateController.StateChanged -= HandleGameStateChanged;
        }

        if (survivalTimer != null)
        {
            survivalTimer.TimeChanged -= HandleTimeChanged;
        }

        if (playerKillCounter != null)
        {
            playerKillCounter.KillCountChanged -= HandleKillCountChanged;
        }
    }

    private void HandleTimeChanged(float elapsedSeconds)
    {
        if (isScore) return;

        int currentSecond = Mathf.FloorToInt(elapsedSeconds);
        // Chỉ tính lại điểm theo thời gian mỗi khi sang giây mới để tránh phát sự kiện mỗi frame (0 GC Alloc)
        if (currentSecond == lastRecordedSecond)
        {
            return;
        }

        lastRecordedSecond = currentSecond;
        RecalculateLiveScore();
    }

    private void HandleKillCountChanged(int newKillCount)
    {
        RecalculateLiveScore();
    }

    private void handleGameStateEnd(bool isVictory)
    {
        CalcutedScore();

        float elapsed = survivalTimer != null ? survivalTimer.ElapsedTime : 0f;
        int kills = playerKillCounter != null ? playerKillCounter.CurrentKills : 0;
        int level = playerLevelSystem != null
            ? Mathf.Max(1, Mathf.RoundToInt(playerLevelSystem.CurrenLevel))
            : 1;

        if (PracticeSaveManager.Instance != null)
        {
            PracticeSaveManager.Instance.RecordCompletedRun(
                isVictory,
                elapsed,
                kills,
                finalScore,
                level
            );
        }
    }

    private void HandleGameStateChanged(PracticeGameStates newState)
    {
        if (newState == PracticeGameStates.Victory)
        {
            handleGameStateEnd(true);
        }
        else if (newState == PracticeGameStates.GameOver)
        {
            handleGameStateEnd(false);
        }
    }

    protected void HandlePlayerDied()
    {
        CalcutedScore();
    }

    public void RecalculateLiveScore()
    {
        if (survivalTimer == null || playerKillCounter == null) return;

        int survivalSecond = Mathf.FloorToInt(survivalTimer.ElapsedTime);
        int currentKills = playerKillCounter.CurrentKills;

        int timeScore = survivalSecond * pointPerSeconds;
        int killScore = currentKills * pointPerKills;

        finalScore = timeScore + killScore;

        ScoreCalculated?.Invoke(finalScore);
    }

    // Giữ nguyên tên hàm CalcutedScore() của bạn để tương thích 100%
    public void CalcutedScore()
    {
        RecalculateLiveScore();
        isScore = true;
        ScoreCalculated?.Invoke(finalScore);
    }
}
