using UnityEngine;
using System;

public class PracticeRunDirector : HaiMonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PracticeSurvivalTimer survivalTimer;

    [SerializeField]
    private PracticeGameStateController gameStateController;

    [Header("Run Settings")]
    [SerializeField, Min(1f)]
    private float runDuration = 5f;
    [SerializeField]
    private PracticeFinalBossController bossController;
    private PracticeRunPhase currentPhase;
    public event Action<PracticeRunPhase> RunPhaseChanged;
    public PracticeRunPhase CurrentPhase =>
        currentPhase;

    public bool IsBossPhase =>
        currentPhase ==
        PracticeRunPhase.BossFight;

    private bool victoryRequested;

    public float RunDuration =>
        runDuration;

    public float RemainingTime
    {
        get
        {
            if (survivalTimer == null)
            {
                return runDuration;
            }

            return Mathf.Max(
                0f,
                runDuration -
                survivalTimer.ElapsedTime
            );
        }
    }

    protected override void LoadComponents()
    {
        base.LoadComponents();

        LoadSurvivalTimer();
        LoadGameStateController();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        LoadSurvivalTimer();
        LoadGameStateController();
        LoadBossController();

        SubscribeBossEvents();

        victoryRequested = false;

        SetRunPhase(PracticeRunPhase.Survival);
    }
    protected override void OnDisable()
    {
        UnsubscribeBossEvents();

        base.OnDisable();
    }

    protected override void Start()
    {
        base.Start();

        Debug.Log(
            $"{name}: Run Director started. " +
            $"Run Duration = {runDuration}, " +
            $"Timer = {survivalTimer}, " +
            $"Game State Controller = {gameStateController}.",
            this
        );
    }

    private void Update()
    {
        CheckRunProgress();
    }

    private void LoadSurvivalTimer()
    {
        if (survivalTimer != null)
        {
            return;
        }

        survivalTimer =
            FindFirstObjectByType<
                PracticeSurvivalTimer
            >();

        if (survivalTimer == null)
        {
            Debug.LogError(
                $"{name}: Không tìm thấy PracticeSurvivalTimer.",
                this
            );
        }
    }
    private void LoadBossController()
    {
        if (bossController != null)
        {
            return;
        }

        bossController =
            FindFirstObjectByType<
                PracticeFinalBossController
            >();

        if (bossController == null)
        {
            Debug.LogError(
                $"{name}: Không tìm thấy " +
                $"PracticeFinalBossController.",
                this
            );
        }
    }

    private void LoadGameStateController()
    {
        if (gameStateController != null)
        {
            return;
        }

        gameStateController =
            FindFirstObjectByType<
                PracticeGameStateController
            >();

        if (gameStateController == null)
        {
            Debug.LogError(
                $"{name}: Không tìm thấy " +
                $"PracticeGameStateController.",
                this
            );
        }
    }

    private void CheckVictoryCondition()
    {
        if (victoryRequested)
        {
            return;
        }

        if (survivalTimer == null ||
            gameStateController == null)
        {
            return;
        }

        if (!gameStateController.IsRunning)
        {
            return;
        }

        if (survivalTimer.ElapsedTime <
            runDuration)
        {
            return;
        }

        Debug.Log(
            $"{name}: Victory condition reached. " +
            $"Elapsed = {survivalTimer.ElapsedTime}, " +
            $"Required = {runDuration}.",
            this
        );

        RequestVictory();
    }
    private void SubscribeBossEvents()
    {
        if (bossController == null)
        {
            return;
        }

        bossController.BossDefeated +=
            HandleBossDefeated;
    }
    private void UnsubscribeBossEvents()
    {
        if (bossController == null)
        {
            return;
        }

        bossController.BossDefeated -=
            HandleBossDefeated;
    }
    private void CheckRunProgress()
    {
        if (survivalTimer == null ||
            gameStateController == null)
        {
            return;
        }

        if (!gameStateController.IsRunning)
        {
            return;
        }

        if (currentPhase !=
            PracticeRunPhase.Survival)
        {
            return;
        }

        if (survivalTimer.ElapsedTime <
            runDuration)
        {
            return;
        }

        StartBossPhase();
    }
    protected void StartBossPhase()
    {
        if (bossController == null) return;
        bool spawnedSuccessfully = bossController.SpawnBoss();

        if (!spawnedSuccessfully)
        {
            Debug.LogWarning($"{name}: Không thể bắt đầu Boss Phase.", this);
            return;
        }

        SetRunPhase(PracticeRunPhase.BossFight);
    }
    protected void HandleBossDefeated()
    {
        if (currentPhase != PracticeRunPhase.BossFight) return;
        if (gameStateController == null || !gameStateController.IsRunning)
        {
            return;
        }

        SetRunPhase(PracticeRunPhase.Completed);

        RequestVictory();
    }

    private void RequestVictory()
    {
        if (victoryRequested)
        {
            return;
        }

        if (gameStateController == null)
        {
            return;
        }

        if (gameStateController.IsVictory)
        {
            victoryRequested = true;
            return;
        }

        victoryRequested = true;

        bool changedSuccessfully =
            gameStateController.RequestVictory();

        if (changedSuccessfully)
        {
            return;
        }

        Debug.LogWarning(
            $"{name}: Victory request was rejected. " +
            $"Current State = " +
            $"{gameStateController.CurrentState}.",
            this
        );
    }
    protected void SetRunPhase(PracticeRunPhase newPhase)
    {
        if (currentPhase == newPhase) return;

        currentPhase = newPhase;

        RunPhaseChanged?.Invoke(currentPhase);
    }
}