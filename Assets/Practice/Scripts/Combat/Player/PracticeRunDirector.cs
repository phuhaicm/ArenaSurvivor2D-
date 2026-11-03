using System;
using UnityEngine;

public class PracticeRunDirector : HaiMonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PracticeSurvivalTimer survivalTimer;

    [SerializeField]
    private PracticeGameStateController gameStateController;

    [SerializeField]
    private PracticeEnemySpawner enemySpawner;

    [SerializeField]
    private PracticeFinalBossController bossController;

    [SerializeField]
    private PracticePlayerKillCounter killCounter;

    [Header("Multi-Wave & Flow Settings")]
    [Tooltip("Bật chế độ Multi-Wave: Sinh tồn Wave 1 -> Mid-Boss (1 cây máu) -> Sinh tồn Wave 2 -> Final Boss (Đa cây máu). Nếu tắt: Chạy 1 đợt thẳng tới Final Boss.")]
    [SerializeField]
    private bool enableMultiWave = true;

    [Tooltip("Thời gian sinh tồn của Wave 1 (giây) trước khi Mid-Boss xuất hiện.")]
    [SerializeField, Min(10f)]
    private float wave1SurvivalDuration = 45f;

    [Tooltip("Hệ số máu của Mid-Boss (Boss Đợt 1). Boss này chỉ có 1 cây máu duy nhất.")]
    [SerializeField, Min(0.2f)]
    private float midBossHealthMultiplier = 0.5f;

    [Tooltip("Thời gian sinh tồn của Wave 2 (giây) trước khi Final Boss xuất hiện.")]
    [SerializeField, Min(10f)]
    private float wave2SurvivalDuration = 55f;

    [Tooltip("Hệ số máu của Final Boss (Boss Đợt 2).")]
    [SerializeField, Min(0.2f)]
    private float finalBossHealthMultiplier = 1.0f;

    [Tooltip("Số Phase tối đa của Final Boss (1 = 2 Phase [0,1], 2 = 3 Phase [0,1,2]).")]
    [SerializeField, Range(0, 3)]
    private int finalBossMaxPhaseIndex = 2;

    private PracticeRunPhase currentPhase;
    public event Action<PracticeRunPhase> RunPhaseChanged;
    public event Action<int> WaveChanged;

    public PracticeRunPhase CurrentPhase => currentPhase;
    public bool IsBossPhase => currentPhase == PracticeRunPhase.BossFight;
    public int CurrentWaveIndex => currentWaveIndex;

    private int currentWaveIndex = 1;
    private float currentWaveStartTime = 0f;
    private bool waveBossTriggered;
    private bool victoryRequested;
    private bool victoryPending;

    public float RunDuration => CurrentWaveTargetDuration;

    public float CurrentWaveTargetDuration
    {
        get
        {
            if (!enableMultiWave)
            {
                return wave1SurvivalDuration + wave2SurvivalDuration;
            }

            return currentWaveIndex == 1 ? wave1SurvivalDuration : wave2SurvivalDuration;
        }
    }

    public float RemainingTime
    {
        get
        {
            if (currentPhase == PracticeRunPhase.BossFight || currentPhase == PracticeRunPhase.Completed)
            {
                return 0f;
            }

            if (survivalTimer == null)
            {
                return CurrentWaveTargetDuration;
            }

            float waveElapsed = survivalTimer.ElapsedTime - currentWaveStartTime;
            return Mathf.Max(0f, CurrentWaveTargetDuration - waveElapsed);
        }
    }

    protected override void LoadComponents()
    {
        base.LoadComponents();
        ResolveReferences();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        ResolveReferences();

        SubscribeBossEvents();
        SubscribeGameStateEvents();

        currentWaveIndex = 1;
        currentWaveStartTime = 0f;
        waveBossTriggered = false;
        victoryRequested = false;
        victoryPending = false;

        SetRunPhase(PracticeRunPhase.Survival);
    }

    protected override void OnDisable()
    {
        UnsubscribeBossEvents();
        UnsubscribeGameStateEvents();

        base.OnDisable();
    }

    protected override void Start()
    {
        base.Start();
        Debug.Log(
            $"{name}: Run Director started. MultiWave = {enableMultiWave}, " +
            $"Wave 1 = {wave1SurvivalDuration}s, Wave 2 = {wave2SurvivalDuration}s.",
            this
        );
    }

    private void Update()
    {
        CheckRunProgress();
    }

    private void ResolveReferences()
    {
        if (survivalTimer == null)
        {
            survivalTimer = FindFirstObjectByType<PracticeSurvivalTimer>();
        }

        if (gameStateController == null)
        {
            gameStateController = FindFirstObjectByType<PracticeGameStateController>();
        }

        if (enemySpawner == null)
        {
            enemySpawner = FindFirstObjectByType<PracticeEnemySpawner>();
        }

        if (bossController == null)
        {
            bossController = FindFirstObjectByType<PracticeFinalBossController>();
        }

        if (killCounter == null)
        {
            killCounter = FindFirstObjectByType<PracticePlayerKillCounter>();
        }
    }

    private void SubscribeGameStateEvents()
    {
        if (gameStateController == null) return;
        gameStateController.StateChanged -= HandleGameStateChanged;
        gameStateController.StateChanged += HandleGameStateChanged;
    }

    private void UnsubscribeGameStateEvents()
    {
        if (gameStateController == null) return;
        gameStateController.StateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(PracticeGameStates newState)
    {
        if (!victoryPending || newState != PracticeGameStates.Running)
        {
            return;
        }

        victoryPending = false;
        RequestVictory();
    }

    private void CheckRunProgress()
    {
        if (survivalTimer == null || gameStateController == null)
        {
            return;
        }

        if (!gameStateController.IsRunning)
        {
            return;
        }

        if (currentPhase != PracticeRunPhase.Survival || waveBossTriggered)
        {
            return;
        }

        float currentWaveElapsed = survivalTimer.ElapsedTime - currentWaveStartTime;

        if (currentWaveIndex == 1 && enableMultiWave)
        {
            if (currentWaveElapsed >= wave1SurvivalDuration)
            {
                StartMidBossPhase();
            }
        }
        else
        {
            float targetDuration = enableMultiWave
                ? wave2SurvivalDuration
                : (wave1SurvivalDuration + wave2SurvivalDuration);

            if (currentWaveElapsed >= targetDuration)
            {
                StartFinalBossPhase();
            }
        }
    }

    protected void StartMidBossPhase()
    {
        if (waveBossTriggered) return;
        waveBossTriggered = true;

        if (enemySpawner != null)
        {
            enemySpawner.SetSpawningEnabled(false);
            enemySpawner.RemoveDestroyedEnemies();
        }

        if (bossController == null)
        {
            Debug.LogError($"{name}: Không tìm thấy PracticeFinalBossController.", this);
            return;
        }

        // Mid-Boss: tiêu đề "MID-BOSS", 1 cây máu duy nhất (allowedMaxPhaseIndex = 0)
        bool spawnedSuccessfully = bossController.SpawnBossEncounter(
            bossTitle: "MID-BOSS",
            healthMultiplier: midBossHealthMultiplier,
            allowedMaxPhaseIndex: 0
        );

        if (!spawnedSuccessfully)
        {
            Debug.LogWarning($"{name}: Không thể triệu hồi Mid-Boss. Chuyển tiếp sang Wave 2.", this);
            TransitionToWave2();
            return;
        }

        SetRunPhase(PracticeRunPhase.BossFight);
    }

    protected void StartFinalBossPhase()
    {
        if (waveBossTriggered) return;
        waveBossTriggered = true;

        if (enemySpawner != null)
        {
            enemySpawner.SetSpawningEnabled(false);
            enemySpawner.RemoveDestroyedEnemies();
        }

        if (bossController == null)
        {
            Debug.LogError($"{name}: Không tìm thấy PracticeFinalBossController.", this);
            return;
        }

        // Final Boss: tiêu đề "FINAL BOSS", đa cây máu (allowedMaxPhaseIndex = finalBossMaxPhaseIndex)
        bool spawnedSuccessfully = bossController.SpawnBossEncounter(
            bossTitle: "FINAL BOSS",
            healthMultiplier: finalBossHealthMultiplier,
            allowedMaxPhaseIndex: finalBossMaxPhaseIndex
        );

        if (!spawnedSuccessfully)
        {
            Debug.LogWarning($"{name}: Không thể bắt đầu Final Boss Phase. Chuyển sang Victory.", this);
            RequestVictory();
            return;
        }

        SetRunPhase(PracticeRunPhase.BossFight);
    }

    private void SubscribeBossEvents()
    {
        if (bossController == null) return;
        bossController.BossDefeated -= HandleBossDefeated;
        bossController.BossDefeated += HandleBossDefeated;
    }

    private void UnsubscribeBossEvents()
    {
        if (bossController == null) return;
        bossController.BossDefeated -= HandleBossDefeated;
    }

    protected void HandleBossDefeated()
    {
        if (currentPhase != PracticeRunPhase.BossFight) return;
        if (gameStateController == null) return;
        if (gameStateController.IsGameOver) return;

        if (killCounter == null)
        {
            killCounter = FindFirstObjectByType<PracticePlayerKillCounter>();
        }
        killCounter?.RegisterKill(isBossKill: true);

        // Nếu vừa đánh bại Mid-Boss của Wave 1 trong chế độ Multi-Wave -> Chuyển sang Wave 2
        if (currentWaveIndex == 1 && enableMultiWave)
        {
            TransitionToWave2();
            return;
        }

        // Đã đánh bại Final Boss -> Hoàn tất trận chiến
        SetRunPhase(PracticeRunPhase.Completed);

        if (!gameStateController.IsRunning)
        {
            victoryPending = true;
            return;
        }

        RequestVictory();
    }

    private void TransitionToWave2()
    {
        currentWaveIndex = 2;
        currentWaveStartTime = survivalTimer != null ? survivalTimer.ElapsedTime : 0f;
        waveBossTriggered = false;

        // Kích hoạt lại việc sinh quái cho Wave 2
        if (enemySpawner != null)
        {
            enemySpawner.SetSpawningEnabled(true);
        }

        killCounter?.ResetBossKillCountedFlag();
        SetRunPhase(PracticeRunPhase.Survival);
        WaveChanged?.Invoke(currentWaveIndex);

        Debug.Log(
            $"<color=#4CAF50><b>[Run Director]</b></color> MID-BOSS HẠ GỤC! " +
            $"Bước vào WAVE 2: Sinh tồn {wave2SurvivalDuration}s trước Final Boss!",
            this
        );
    }

    protected void RequestVictory()
    {
        if (victoryRequested) return;

        if (gameStateController == null)
        {
            Debug.LogError($"{name}: Không tìm thấy PracticeGameStateController.", this);
            return;
        }

        victoryRequested = true;

        bool victoryStarted = gameStateController.RequestVictory();
        if (!victoryStarted && !gameStateController.IsVictory)
        {
            Debug.LogWarning($"{name}: Yêu cầu Victory bị từ chối.", this);
        }
    }

    private void SetRunPhase(PracticeRunPhase newPhase)
    {
        if (currentPhase == newPhase) return;

        currentPhase = newPhase;
        RunPhaseChanged?.Invoke(currentPhase);
    }
}