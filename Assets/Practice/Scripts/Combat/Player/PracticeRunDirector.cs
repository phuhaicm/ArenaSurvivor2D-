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

    [Header("Multi-Wave & Flow Settings (5-Minute Standard Pacing)")]
    [Tooltip("Bật chế độ Multi-Wave: Sinh tồn Wave 1 -> Mid-Boss (1 cây máu) -> Sinh tồn Wave 2 -> Final Boss (Đa cây máu).")]
    [SerializeField]
    private bool enableMultiWave = true;

    [Tooltip("Thời gian sinh tồn của Wave 1 (giây) trước khi Mid-Boss xuất hiện (Chuẩn: 120s).")]
    [SerializeField, Min(10f)]
    private float wave1SurvivalDuration = 120f;

    [Tooltip("Hệ số máu của Mid-Boss (Boss Đợt 1). Boss này chỉ có 1 cây máu duy nhất.")]
    [SerializeField, Min(0.2f)]
    private float midBossHealthMultiplier = 0.6f;

    [Tooltip("Thời gian sinh tồn của Wave 2 (giây) trước khi Final Boss xuất hiện (Chuẩn: 150s).")]
    [SerializeField, Min(10f)]
    private float wave2SurvivalDuration = 150f;

    [Tooltip("Hệ số máu của Final Boss (Boss Đợt 2).")]
    [SerializeField, Min(0.2f)]
    private float finalBossHealthMultiplier = 1.2f;

    [Tooltip("Số Phase tối đa của Final Boss (1 = 2 Phase [0,1], 2 = 3 Phase [0,1,2]).")]
    [SerializeField, Range(0, 3)]
    private int finalBossMaxPhaseIndex = 2;

    [Header("Dynamic Wave Events (Elite & Ambush Swarm)")]
    [SerializeField]
    private bool enableDynamicWaveEvents = true;

    [SerializeField, Range(6, 36)]
    private int wave1AmbushCount = 14;

    [SerializeField, Range(8, 48)]
    private int wave2AmbushCount = 22;

    [SerializeField, Min(4f)]
    private float ambushRingRadius = 7.5f;

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

    // Cờ theo dõi các sự kiện đột kích giữa mỗi Wave (chỉ kích hoạt đúng 1 lần mỗi mốc)
    private bool waveElite1Spawned;
    private bool waveAmbushTriggered;
    private bool waveElite2Spawned;

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
        ResetWaveEventFlags();

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

    private void ResetWaveEventFlags()
    {
        waveElite1Spawned = false;
        waveAmbushTriggered = false;
        waveElite2Spawned = false;
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
        float targetDuration = CurrentWaveTargetDuration;

        if (enableDynamicWaveEvents && targetDuration > 15f)
        {
            EvaluateDynamicWaveEvents(currentWaveElapsed, targetDuration);
        }

        if (currentWaveIndex == 1 && enableMultiWave)
        {
            if (currentWaveElapsed >= wave1SurvivalDuration)
            {
                StartMidBossPhase();
            }
        }
        else
        {
            if (currentWaveElapsed >= targetDuration)
            {
                StartFinalBossPhase();
            }
        }
    }

    private void EvaluateDynamicWaveEvents(float waveElapsed, float waveDuration)
    {
        if (enemySpawner == null) return;

        float progressRatio = waveElapsed / waveDuration;

        // Mốc 35% thời gian của Wave: Triệu hồi Quái Tinh Anh (Elite) đầu tiên
        if (!waveElite1Spawned && progressRatio >= 0.35f)
        {
            waveElite1Spawned = true;
            enemySpawner.SpawnEliteEnemy();
            Debug.Log($"<color=#FF9100><b>[Wave {currentWaveIndex} Event]</b></color> ELITE ENEMY XUẤT HIỆN!", this);
        }

        // Mốc 60% thời gian của Wave: Kích hoạt Vòng Vây 360 Độ (Ambush Ring Swarm)
        if (!waveAmbushTriggered && progressRatio >= 0.60f)
        {
            waveAmbushTriggered = true;
            int ambushCount = currentWaveIndex == 1 ? wave1AmbushCount : wave2AmbushCount;
            int spawned = enemySpawner.SpawnAmbushRing(ambushCount, ambushRingRadius);
            Debug.Log(
                $"<color=#FF1744><b>[Wave {currentWaveIndex} Event]</b></color> AMBUSH SWARM! {spawned} quái bao vây Player!",
                this
            );
        }

        // Mốc 80% thời gian của Wave: Triệu hồi Quái Tinh Anh (Elite) thứ hai để người chơi gom mưa XP trước khi gặp Boss
        if (!waveElite2Spawned && progressRatio >= 0.80f)
        {
            waveElite2Spawned = true;
            enemySpawner.SpawnEliteEnemy();
            if (currentWaveIndex >= 2)
            {
                // Ở Wave 2 sinh liền 2 con Elite cùng lúc để tăng độ kịch tính
                enemySpawner.SpawnEliteEnemy();
            }
            Debug.Log($"<color=#FF9100><b>[Wave {currentWaveIndex} Event]</b></color> ELITE REINFORCEMENT XUẤT HIỆN!", this);
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

        if (currentWaveIndex == 1 && enableMultiWave)
        {
            TransitionToWave2();
            return;
        }

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
        ResetWaveEventFlags();

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