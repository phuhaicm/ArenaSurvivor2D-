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
    private bool attackInProgress;
    private int currentPhaseIndex = -1;
    private PracticeBossAttackDefinition lastSelectedAttack;

    public event Action<int, PracticeBossPhaseDefinition> PhaseChanged;

    public int CurrentPhaseIndex => currentPhaseIndex;
    public bool IsAttackInProgress => attackInProgress;
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

        ResetRuntimePhaseState();
        SubscribeBossEvents();
        ResetAttackPresentation();
        StartAttackLoop();
    }

    protected override void OnDisable()
    {
        UnsubscribeBossEvents();
        StopAttackLoop();
        ResetAttackPresentation();

        base.OnDisable();
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

        if (attackExecutors.Count > 0)
        {
            return;
        }

        PracticeBossAttackExecutorBase[] foundExecutors =
            GetComponentsInChildren<PracticeBossAttackExecutorBase>(true);

        if (foundExecutors != null && foundExecutors.Length > 0)
        {
            attackExecutors.AddRange(foundExecutors);
        }
    }

    private void SubscribeBossEvents()
    {
        if (bossHealth == null)
        {
            return;
        }

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
        }
    }

    private void HandleBossDamageTaken(float damageAmount)
    {
        if (!CanContinueAttacking())
        {
            return;
        }

        EvaluatePhaseTransition();
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

        if (!CanContinueAttacking())
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
            $"{name}: Không tìm thấy Executor cho kiểu {attack.PatternType}. Vui lòng gắn Executor lên Boss Prefab.",
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

    private IEnumerator WaitUntilGameIsRunning()
    {
        while (gameStateController != null && !gameStateController.IsRunning)
        {
            if (!CanContinueAttacking())
            {
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator WaitUntilRunningForSeconds(float duration)
    {
        if (duration <= 0f)
        {
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (!CanContinueAttacking())
            {
                yield break;
            }

            if (gameStateController != null && gameStateController.IsRunning)
            {
                elapsedTime += Time.deltaTime;
            }

            yield return null;
        }
    }

    private bool CanContinueAttacking()
    {
        if (!isActiveAndEnabled)
        {
            return false;
        }

        if (bossHealth == null || bossHealth.IsDead)
        {
            return false;
        }

        if (gameStateController == null)
        {
            return false;
        }

        if (gameStateController.IsGameOver || gameStateController.IsVictory)
        {
            return false;
        }

        return true;
    }

    private void HandleBossDied()
    {
        StopAttackLoop();
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

        for (int i = projectileContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = projectileContainer.GetChild(i);

            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void StopAttackLoop()
    {
        if (attackLoopCoroutine == null)
        {
            return;
        }

        StopCoroutine(attackLoopCoroutine);

        attackLoopCoroutine = null;
        attackInProgress = false;
    }

    private void ResetAttackPresentation()
    {
        SetTelegraphActive(false);
    }

    private void SetTelegraphActive(bool isActive)
    {
        if (telegraphRenderer == null)
        {
            return;
        }

        telegraphRenderer.enabled = isActive;
    }
}