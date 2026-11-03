using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PracticeBossAttackController : HaiMonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PracticeEnemyHealth bossHealth;

    [SerializeField]
    private PracticeGameStateController gameStateController;

    [SerializeField]
    private Transform firePoint;

    [SerializeField]
    private SpriteRenderer telegraphRenderer;

    [SerializeField]
    private SpriteRenderer bossBodyRenderer;

    [SerializeField]
    private Transform projectileContainer;

    [Header("Attack Executors (Strategy Modules)")]
    [SerializeField]
    private List<PracticeBossAttackExecutorBase> attackExecutors =
        new List<PracticeBossAttackExecutorBase>();

    [Header("Combat Timing")]
    [Min(0f)]
    [SerializeField]
    private float initialCombatDelay = 1.25f;

    [Header("Multi-Phase Health Bar Settings")]
    [Tooltip("Nếu bật: Mỗi Phase có 1 thanh máu 100% riêng biệt. Đánh hết thanh máu Phase 1 sẽ chuyển sang Phase 2 và nạp đầy thanh máu mới!")]
    [SerializeField]
    private bool useSeparateHealthBarPerPhase = true;

    [Tooltip("Thời gian Boss bất tử và ngừng bắn để biểu diễn hiệu ứng chuyển Phase (giây).")]
    [Min(0f)]
    [SerializeField]
    private float phaseTransitionDuration = 1.25f;

    [Header("Phase Data")]
    [SerializeField]
    private List<PracticeBossPhaseDefinition> phases =
        new List<PracticeBossPhaseDefinition>();

    [Header("Fallback Single Attack (Optional)")]
    [SerializeField]
    private PracticeBossAttackDefinition fallbackAttackDefinition;

    [Header("Victory Cleanup")]
    [SerializeField]
    private bool clearBossProjectilesOnDeath = true;

    private Coroutine attackLoopCoroutine;
    private Coroutine phaseTransitionCoroutine;
    private bool attackInProgress;
    private bool isTransitioningPhase;
    private int currentPhaseIndex = -1;
    private int maxAllowedPhaseIndex = 99;
    private float encounterHealthMultiplier = 1f;
    private PracticeBossAttackDefinition lastSelectedAttack;

    public event Action<int, PracticeBossPhaseDefinition> PhaseChanged;

    public int CurrentPhaseIndex => currentPhaseIndex;
    public int MaxAllowedPhaseIndex => maxAllowedPhaseIndex;
    public bool IsAttackInProgress => attackInProgress;
    public bool IsTransitioningPhase => isTransitioningPhase;

    public PracticeBossPhaseDefinition CurrentPhase
    {
        get
        {
            if (currentPhaseIndex < 0 || currentPhaseIndex >= phases.Count)
            {
                return null;
            }

            return phases[currentPhaseIndex];
        }
    }

    protected override void LoadComponents()
    {
        base.LoadComponents();

        LoadBossHealth();
        LoadGameStateController();
        LoadBossBodyRenderer();
        LoadProjectileContainer();
        LoadAttackExecutors();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        LoadBossHealth();
        LoadGameStateController();
        LoadBossBodyRenderer();
        LoadProjectileContainer();
        LoadAttackExecutors();

        isTransitioningPhase = false;
        encounterHealthMultiplier = 1f;
        maxAllowedPhaseIndex = phases != null && phases.Count > 0
            ? phases.Count - 1
            : 0;

        ResetRuntimePhaseState();
        SubscribeBossEvents();
        ResetAttackPresentation();
        StartAttackLoop();
    }

    protected override void OnDisable()
    {
        UnsubscribeBossEvents();
        StopAttackLoop();
        StopPhaseTransitionRoutine();
        ResetAttackPresentation();

        base.OnDisable();
    }

    // Được gọi bởi PracticeFinalBossController khi sinh Boss cho từng Đợt (Wave 1 Mid-Boss vs Wave 2 Final Boss)
    public void ConfigureEncounterRules(
        float healthMultiplier,
        int allowedMaxPhaseIndex
    )
    {
        LoadBossHealth();

        encounterHealthMultiplier = Mathf.Max(0.2f, healthMultiplier);

        int totalPhases = phases != null ? phases.Count : 0;
        if (totalPhases > 0)
        {
            maxAllowedPhaseIndex = allowedMaxPhaseIndex < 0
                ? totalPhases - 1
                : Mathf.Clamp(allowedMaxPhaseIndex, 0, totalPhases - 1);
        }
        else
        {
            maxAllowedPhaseIndex = 0;
        }

        currentPhaseIndex = -1;
        isTransitioningPhase = false;

        if (totalPhases > 0)
        {
            SetPhase(0);
            ApplyPhaseHealthBar(0);
        }
        else if (bossHealth != null)
        {
            bossHealth.ConfigureMaxHealthAndRefill(
                bossHealth.BaseMaxHealth * encounterHealthMultiplier
            );
        }
    }

    private void LoadBossHealth()
    {
        if (bossHealth != null)
        {
            return;
        }

        bossHealth = GetComponent<PracticeEnemyHealth>();
    }

    private void LoadGameStateController()
    {
        if (gameStateController != null)
        {
            return;
        }

        gameStateController = FindFirstObjectByType<PracticeGameStateController>();
    }

    private void LoadBossBodyRenderer()
    {
        if (bossBodyRenderer != null)
        {
            return;
        }

        bossBodyRenderer = GetComponent<SpriteRenderer>();
    }

    private void LoadProjectileContainer()
    {
        if (projectileContainer != null)
        {
            return;
        }

        GameObject containerObject = GameObject.Find("RuntimeBossProjectiles");

        if (containerObject != null)
        {
            projectileContainer = containerObject.transform;
        }
    }

    private void LoadAttackExecutors()
    {
        attackExecutors.RemoveAll(executor => executor == null);

        PracticeBossAttackExecutorBase[] foundExecutors =
            GetComponentsInChildren<PracticeBossAttackExecutorBase>(true);

        if (foundExecutors != null && foundExecutors.Length > 0)
        {
            for (int i = 0; i < foundExecutors.Length; i++)
            {
                if (!attackExecutors.Contains(foundExecutors[i]))
                {
                    attackExecutors.Add(foundExecutors[i]);
                }
            }
        }

        if (FindExecutorForPattern(PracticeBossAttackPatternType.RadialBurst) == null)
        {
            PracticeBossRadialAttackExecutor radialExecutor =
                gameObject.GetComponent<PracticeBossRadialAttackExecutor>();

            if (radialExecutor == null)
            {
                radialExecutor = gameObject.AddComponent<PracticeBossRadialAttackExecutor>();
            }

            attackExecutors.Add(radialExecutor);
        }

        if (FindExecutorForPattern(PracticeBossAttackPatternType.AimedSpread) == null)
        {
            PracticeBossAimedSpreadAttackExecutor aimedExecutor =
                gameObject.GetComponent<PracticeBossAimedSpreadAttackExecutor>();

            if (aimedExecutor == null)
            {
                aimedExecutor = gameObject.AddComponent<PracticeBossAimedSpreadAttackExecutor>();
            }

            attackExecutors.Add(aimedExecutor);
        }
    }

    private void SubscribeBossEvents()
    {
        if (bossHealth == null)
        {
            return;
        }

        bossHealth.LethalDamageInterceptor = TryInterceptLethalDamageForNextPhase;

        bossHealth.Died -= HandleBossDied;
        bossHealth.Died += HandleBossDied;

        bossHealth.DamageTaken -= HandleBossDamageTaken;
        bossHealth.DamageTaken += HandleBossDamageTaken;
    }

    private void UnsubscribeBossEvents()
    {
        if (bossHealth == null)
        {
            return;
        }

        if (bossHealth.LethalDamageInterceptor == TryInterceptLethalDamageForNextPhase)
        {
            bossHealth.LethalDamageInterceptor = null;
        }

        bossHealth.Died -= HandleBossDied;
        bossHealth.DamageTaken -= HandleBossDamageTaken;
    }

    private void ResetRuntimePhaseState()
    {
        currentPhaseIndex = -1;
        lastSelectedAttack = null;

        if (phases != null && phases.Count > 0)
        {
            SetPhase(0);
            ApplyPhaseHealthBar(0);
        }
    }

    // Chặn đòn kết liễu nếu Boss vẫn còn Phase tiếp theo trong đợt giao tranh này
    private bool TryInterceptLethalDamageForNextPhase()
    {
        if (!useSeparateHealthBarPerPhase || phases == null || phases.Count <= 1)
        {
            return false;
        }

        int nextPhaseIndex = currentPhaseIndex + 1;
        int upperPhaseLimit = Mathf.Min(maxAllowedPhaseIndex, phases.Count - 1);

        // Nếu đã ở Phase cuối cùng được phép của Đợt này -> Cho phép Boss chết thật sự!
        if (nextPhaseIndex > upperPhaseLimit)
        {
            return false;
        }

        // Vẫn còn Phase tiếp theo -> Chuyển sang Phase mới và nạp lại đầy thanh máu!
        TransitionToNextPhaseBar(nextPhaseIndex);
        return true;
    }

    private void TransitionToNextPhaseBar(int nextPhaseIndex)
    {
        // Dọn sạch đạn cũ của Phase trước để người chơi có nhịp thở công bằng
        ClearActiveBossProjectiles();
        SetTelegraphActive(false);

        // Nạp đầy thanh máu mới cho Phase tiếp theo trước khi phát sự kiện PhaseChanged
        // để PracticeBossPresentationUI đọc được ngay bình máu 100% mới!
        ApplyPhaseHealthBar(nextPhaseIndex);
        SetPhase(nextPhaseIndex);

        if (phaseTransitionDuration > 0f && isActiveAndEnabled)
        {
            StopPhaseTransitionRoutine();
            phaseTransitionCoroutine = StartCoroutine(PhaseTransitionPauseRoutine());
        }
    }

    private IEnumerator PhaseTransitionPauseRoutine()
    {
        isTransitioningPhase = true;

        if (bossHealth != null)
        {
            bossHealth.SetInvulnerable(true);
        }

        yield return WaitUntilRunningForSeconds(phaseTransitionDuration);

        if (bossHealth != null)
        {
            bossHealth.SetInvulnerable(false);
        }

        isTransitioningPhase = false;
        phaseTransitionCoroutine = null;
    }

    private void StopPhaseTransitionRoutine()
    {
        if (phaseTransitionCoroutine != null)
        {
            StopCoroutine(phaseTransitionCoroutine);
            phaseTransitionCoroutine = null;
        }

        isTransitioningPhase = false;

        if (bossHealth != null)
        {
            bossHealth.SetInvulnerable(false);
        }
    }

    private void ApplyPhaseHealthBar(int phaseIndex)
    {
        if (bossHealth == null)
        {
            return;
        }

        float phaseMultiplier = 1f;

        if (phases != null &&
            phaseIndex >= 0 &&
            phaseIndex < phases.Count &&
            phases[phaseIndex] != null)
        {
            phaseMultiplier = phases[phaseIndex].PhaseHealthMultiplier;
        }

        float targetMaxHealth =
            bossHealth.BaseMaxHealth * encounterHealthMultiplier * phaseMultiplier;

        bossHealth.ConfigureMaxHealthAndRefill(targetMaxHealth);
    }

    private void HandleBossDamageTaken(float damageAmount)
    {
        if (!CanContinueAttacking())
        {
            return;
        }

        // Chỉ đánh giá chuyển Phase theo % máu nếu tắt chế độ Multi-Bar
        if (!useSeparateHealthBarPerPhase)
        {
            EvaluatePhaseTransition();
        }
    }

    

    private float GetNormalizedHealth()
    {
        if (bossHealth == null || bossHealth.MaxHealth <= 0f)
        {
            return 1f;
        }

        return Mathf.Clamp01(bossHealth.CurrentHealth / bossHealth.MaxHealth);
    }

    private void SetPhase(int newPhaseIndex)
    {
        if (newPhaseIndex < 0 || newPhaseIndex >= phases.Count)
        {
            return;
        }

        if (currentPhaseIndex == newPhaseIndex)
        {
            return;
        }

        currentPhaseIndex = newPhaseIndex;
        lastSelectedAttack = null;

        PracticeBossPhaseDefinition activePhase = CurrentPhase;
        ApplyPhasePresentation(activePhase);

        PhaseChanged?.Invoke(currentPhaseIndex, activePhase);
    }

    private void ApplyPhasePresentation(PracticeBossPhaseDefinition phase)
    {
        if (phase == null)
        {
            return;
        }

        if (bossBodyRenderer != null)
        {
            bossBodyRenderer.color = phase.PhaseTintColor;
        }

        if (telegraphRenderer != null)
        {
            telegraphRenderer.color = phase.TelegraphColor;
        }
    }

    private void StartAttackLoop()
    {
        if (!CanStartAttackLoop())
        {
            return;
        }

        StopAttackLoop();

        attackLoopCoroutine = StartCoroutine(AttackLoopRoutine());
    }

    private bool CanStartAttackLoop()
    {
        if (firePoint == null)
        {
            Debug.LogError($"{name}: Fire Point đang null.", this);
            return false;
        }

        if (bossHealth == null || bossHealth.IsDead)
        {
            return false;
        }

        bool hasValidPhase = phases != null && phases.Count > 0 && phases[0] != null && phases[0].HasValidAttacks();
        bool hasValidFallback = fallbackAttackDefinition != null && fallbackAttackDefinition.ProjectilePrefab != null;

        if (!hasValidPhase && !hasValidFallback)
        {
            Debug.LogError($"{name}: Chưa cấu hình Phase hợp lệ hoặc Fallback Attack Definition.", this);
            return false;
        }

        return true;
    }

    private IEnumerator AttackLoopRoutine()
    {
        if (initialCombatDelay > 0f)
        {
            yield return WaitUntilRunningForSeconds(initialCombatDelay);
        }

        while (CanContinueAttacking())
        {
            yield return WaitUntilGameIsRunning();

            if (!CanContinueAttacking())
            {
                break;
            }

            // Chờ nếu Boss đang trong 1.25s gồng mình chuyển Phase
            while (isTransitioningPhase && CanContinueAttacking())
            {
                yield return null;
            }

            PracticeBossAttackDefinition selectedAttack = SelectNextAttack();

            if (selectedAttack == null || selectedAttack.ProjectilePrefab == null)
            {
                yield return null;
                continue;
            }

            yield return PerformAttackRoutine(selectedAttack);

            if (!CanContinueAttacking())
            {
                break;
            }

            float runtimeCooldown = CalculateRuntimeCooldown(selectedAttack);
            yield return WaitUntilRunningForSeconds(runtimeCooldown);
        }

        attackInProgress = false;
        attackLoopCoroutine = null;
    }

    private PracticeBossAttackDefinition SelectNextAttack()
    {
        PracticeBossPhaseDefinition activePhase = CurrentPhase;

        if (activePhase == null || !activePhase.HasValidAttacks())
        {
            return fallbackAttackDefinition;
        }

        IReadOnlyList<PracticeBossAttackDefinition> phaseAttacks = activePhase.Attacks;
        List<PracticeBossAttackDefinition> validCandidates = new List<PracticeBossAttackDefinition>();

        for (int i = 0; i < phaseAttacks.Count; i++)
        {
            PracticeBossAttackDefinition attack = phaseAttacks[i];

            if (attack != null && attack.ProjectilePrefab != null)
            {
                validCandidates.Add(attack);
            }
        }

        if (validCandidates.Count == 0)
        {
            return fallbackAttackDefinition;
        }

        if (validCandidates.Count == 1)
        {
            lastSelectedAttack = validCandidates[0];
            return lastSelectedAttack;
        }

        List<PracticeBossAttackDefinition> nonRepeatingCandidates = new List<PracticeBossAttackDefinition>();

        for (int i = 0; i < validCandidates.Count; i++)
        {
            if (validCandidates[i] != lastSelectedAttack)
            {
                nonRepeatingCandidates.Add(validCandidates[i]);
            }
        }

        List<PracticeBossAttackDefinition> poolToPick =
            nonRepeatingCandidates.Count > 0 ? nonRepeatingCandidates : validCandidates;

        int randomIndex = UnityEngine.Random.Range(0, poolToPick.Count);
        lastSelectedAttack = poolToPick[randomIndex];

        return lastSelectedAttack;
    }

    private float CalculateRuntimeCooldown(PracticeBossAttackDefinition attack)
    {
        if (attack == null)
        {
            return 1f;
        }

        float multiplier = CurrentPhase != null ? CurrentPhase.CooldownMultiplier : 1f;
        return Mathf.Max(0.1f, attack.AttackCooldown * multiplier);
    }

    private IEnumerator PerformAttackRoutine(PracticeBossAttackDefinition attack)
    {
        attackInProgress = true;

        SetTelegraphActive(true);

        if (attack.TelegraphDuration > 0f)
        {
            yield return WaitUntilRunningForSeconds(attack.TelegraphDuration);
        }

        SetTelegraphActive(false);

        if (!CanContinueAttacking() || isTransitioningPhase)
        {
            attackInProgress = false;
            yield break;
        }

        ExecuteAttackByStrategy(attack);

        attackInProgress = false;
    }

    private void ExecuteAttackByStrategy(PracticeBossAttackDefinition attack)
    {
        if (attack == null)
        {
            return;
        }

        PracticeBossAttackExecutorBase executor = FindExecutorForPattern(attack.PatternType);

        if (executor != null)
        {
            executor.ExecuteAttack(attack, firePoint, projectileContainer);
            return;
        }

        Debug.LogWarning(
            $"{name}: Không tìm thấy Executor cho kiểu bắn {attack.PatternType}.",
            this
        );
    }

    private PracticeBossAttackExecutorBase FindExecutorForPattern(
        PracticeBossAttackPatternType patternType
    )
    {
        for (int i = 0; i < attackExecutors.Count; i++)
        {
            PracticeBossAttackExecutorBase executor = attackExecutors[i];

            if (executor != null && executor.SupportedPatternType == patternType)
            {
                return executor;
            }
        }

        return null;
    }

    private void StopAttackLoop()
    {
        if (attackLoopCoroutine != null)
        {
            StopCoroutine(attackLoopCoroutine);
            attackLoopCoroutine = null;
        }

        attackInProgress = false;
    }

    private bool CanContinueAttacking()
    {
        return bossHealth != null && !bossHealth.IsDead;
    }

    private IEnumerator WaitUntilGameIsRunning()
    {
        while (gameStateController != null && !gameStateController.IsRunning)
        {
            yield return null;
        }
    }

    private IEnumerator WaitUntilRunningForSeconds(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (!CanContinueAttacking())
            {
                yield break;
            }

            if (gameStateController == null || gameStateController.IsRunning)
            {
                elapsed += Time.deltaTime;
            }

            yield return null;
        }
    }

    private void SetTelegraphActive(bool isActive)
    {
        if (telegraphRenderer != null)
        {
            telegraphRenderer.enabled = isActive;
        }
    }

    private void ResetAttackPresentation()
    {
        SetTelegraphActive(false);
    }

    private void HandleBossDied()
    {
        StopAttackLoop();
        StopPhaseTransitionRoutine();
        ResetAttackPresentation();

        if (clearBossProjectilesOnDeath)
        {
            ClearActiveBossProjectiles();
        }
    }

    private void ClearActiveBossProjectiles()
    {
        if (projectileContainer == null)
        {
            return;
        }

        if (PracticeObjectPoolManager.Instance != null)
        {
            PracticeObjectPoolManager.Instance.DespawnAllActiveInContainer(projectileContainer);
            return;
        }

        for (int i = projectileContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = projectileContainer.GetChild(i);

            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }
    }
    private void EvaluatePhaseTransition()
    {
        if (phases == null || phases.Count == 0)
        {
            return;
        }

        float normalizedHealth = GetNormalizedHealth();
        int targetPhaseIndex = currentPhaseIndex;

        for (int i = currentPhaseIndex + 1; i < phases.Count; i++)
        {
            PracticeBossPhaseDefinition candidatePhase = phases[i];

            if (candidatePhase == null)
            {
                continue;
            }

            // Khi máu tụt xuống dưới ngưỡng HealthThreshold của Phase tiếp theo (ví dụ 0.5 cho Phase 2)
            if (normalizedHealth <= candidatePhase.HealthThreshold)
            {
                targetPhaseIndex = i;
            }
        }

        if (targetPhaseIndex != currentPhaseIndex && targetPhaseIndex >= 0)
        {
            SetPhase(targetPhaseIndex);
        }
    }
}