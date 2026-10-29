using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PracticeBossPresentationUI : HaiMonoBehaviour
{
    [Header("Boss System")]
    [SerializeField]
    private PracticeFinalBossController bossController;

    [Header("Warning UI")]
    [SerializeField]
    private GameObject warningPanel;

    [SerializeField]
    private CanvasGroup warningCanvasGroup;

    [SerializeField]
    private TextMeshProUGUI warningText;

    [SerializeField, Min(0f)]
    private float warningFadeDuration = 0.3f;

    [SerializeField, Min(0f)]
    private float warningHoldDuration = 1.5f;

    [Header("Boss Health UI")]
    [SerializeField]
    private GameObject bossHealthPanel;

    [SerializeField]
    private TextMeshProUGUI bossNameText;

    [SerializeField]
    private Slider bossHealthSlider;

    [SerializeField]
    private TextMeshProUGUI bossHealthText;

    [Header("Display")]
    [SerializeField]
    private string bossDisplayName = "FINAL BOSS";

    private PracticeEnemyHealth activeBoss;

    private PracticeBossAttackController
        activeBossAttackController;

    private Coroutine warningCoroutine;

    protected override void LoadComponents()
    {
        base.LoadComponents();

        LoadBossController();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        LoadBossController();
        SubscribeBossControllerEvents();

        InitializePresentation();

        if (bossController != null &&
            bossController.HasActiveBoss)
        {
            HandleBossSpawned();
        }
    }

    protected override void OnDisable()
    {
        UnsubscribeBossControllerEvents();
        UnbindCurrentBoss();
        StopWarning();

        base.OnDisable();
    }

    // =========================================================
    // LOAD REFERENCES
    // =========================================================

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
    }

    // =========================================================
    // INITIALIZATION
    // =========================================================

    private void InitializePresentation()
    {
        SetBossHealthPanelActive(false);

        if (bossNameText != null)
        {
            bossNameText.text =
                bossDisplayName;
        }

        if (warningText != null)
        {
            warningText.text =
                bossDisplayName;
        }

        if (warningCanvasGroup != null)
        {
            warningCanvasGroup.alpha = 0f;
            warningCanvasGroup.interactable = false;
            warningCanvasGroup.blocksRaycasts = false;
        }

        SetWarningPanelActive(false);
        ConfigureHealthSlider();
    }

    private void ConfigureHealthSlider()
    {
        if (bossHealthSlider == null)
        {
            return;
        }

        bossHealthSlider.minValue = 0f;
        bossHealthSlider.maxValue = 1f;
        bossHealthSlider.wholeNumbers = false;
        bossHealthSlider.interactable = false;
        bossHealthSlider.value = 1f;
    }

    // =========================================================
    // BOSS CONTROLLER EVENTS
    // =========================================================

    private void SubscribeBossControllerEvents()
    {
        if (bossController == null)
        {
            return;
        }

        bossController.BossSpawned -=
            HandleBossSpawned;

        bossController.BossSpawned +=
            HandleBossSpawned;
    }

    private void UnsubscribeBossControllerEvents()
    {
        if (bossController == null)
        {
            return;
        }

        bossController.BossSpawned -=
            HandleBossSpawned;
    }

    private void HandleBossSpawned()
    {
        AttachActiveBoss();

        if (activeBoss == null)
        {
            SetBossHealthPanelActive(false);
            return;
        }

        RefreshBossPresentation();
        RefreshCurrentBossPhase();

        SetBossHealthPanelActive(true);

        PlayWarning(bossDisplayName);
    }

    // =========================================================
    // ACTIVE BOSS BINDING
    // =========================================================

    private void AttachActiveBoss()
    {
        UnbindCurrentBoss();

        if (bossController == null ||
            !bossController.HasActiveBoss)
        {
            return;
        }

        activeBoss =
            bossController.ActiveBoss;

        if (activeBoss == null)
        {
            return;
        }

        SubscribeActiveBossHealthEvents();
        BindBossAttackEvents(activeBoss);
    }

    private void UnbindCurrentBoss()
    {
        UnsubscribeActiveBossHealthEvents();
        UnbindBossAttackEvents();

        activeBoss = null;
    }

    // =========================================================
    // ACTIVE BOSS HEALTH EVENTS
    // =========================================================

    private void SubscribeActiveBossHealthEvents()
    {
        if (activeBoss == null)
        {
            return;
        }

        activeBoss.damageTaken -=
            HandleBossDamageTaken;

        activeBoss.damageTaken +=
            HandleBossDamageTaken;

        activeBoss.Died -=
            HandleBossDied;

        activeBoss.Died +=
            HandleBossDied;
    }

    private void UnsubscribeActiveBossHealthEvents()
    {
        if (activeBoss == null)
        {
            return;
        }

        activeBoss.damageTaken -=
            HandleBossDamageTaken;

        activeBoss.Died -=
            HandleBossDied;
    }

    private void HandleBossDamageTaken(
        float appliedDamage
    )
    {
        RefreshBossHealth();
    }

    private void HandleBossDied()
    {
        RefreshBossHealth();

        UnsubscribeActiveBossHealthEvents();
        UnbindBossAttackEvents();

        activeBoss = null;

        StopWarning();
        SetBossHealthPanelActive(false);
    }

    // =========================================================
    // BOSS ATTACK / PHASE EVENTS
    // =========================================================

    private void BindBossAttackEvents(
        PracticeEnemyHealth bossHealth
    )
    {
        UnbindBossAttackEvents();

        if (bossHealth == null)
        {
            return;
        }

        activeBossAttackController =
            bossHealth.GetComponent<
                PracticeBossAttackController
            >();

        if (activeBossAttackController == null)
        {
            Debug.LogWarning(
                $"{name}: Active Boss không có " +
                $"{nameof(PracticeBossAttackController)}.",
                bossHealth
            );

            return;
        }

        activeBossAttackController.PhaseChanged -=
            HandleBossPhaseChanged;

        activeBossAttackController.PhaseChanged +=
            HandleBossPhaseChanged;
    }

    private void UnbindBossAttackEvents()
    {
        if (activeBossAttackController == null)
        {
            return;
        }

        activeBossAttackController.PhaseChanged -=
            HandleBossPhaseChanged;

        activeBossAttackController = null;
    }

    private void RefreshCurrentBossPhase()
    {
        if (activeBossAttackController == null)
        {
            return;
        }

        PracticeBossPhaseDefinition currentPhase =
            activeBossAttackController.CurrentPhase;

        if (currentPhase == null)
        {
            return;
        }

        UpdateBossPhaseTitle(currentPhase);
    }

    private void HandleBossPhaseChanged(
        int phaseIndex,
        PracticeBossPhaseDefinition phaseDefinition
    )
    {
        if (phaseDefinition == null)
        {
            return;
        }

        UpdateBossPhaseTitle(phaseDefinition);

        if (phaseIndex > 0)
        {
            ShowPhaseTransitionWarning(
                phaseDefinition
            );
        }
    }

    private void UpdateBossPhaseTitle(
        PracticeBossPhaseDefinition phaseDefinition
    )
    {
        if (bossNameText == null ||
            phaseDefinition == null)
        {
            return;
        }

        string phaseName =
            phaseDefinition.PhaseName;

        if (string.IsNullOrWhiteSpace(phaseName))
        {
            bossNameText.text =
                bossDisplayName;

            return;
        }

        bossNameText.text =
            $"{bossDisplayName} - " +
            $"{phaseName.ToUpperInvariant()}";
    }

    private void ShowPhaseTransitionWarning(
        PracticeBossPhaseDefinition phaseDefinition
    )
    {
        if (phaseDefinition == null)
        {
            return;
        }

        string phaseName =
            phaseDefinition.PhaseName;

        if (string.IsNullOrWhiteSpace(phaseName))
        {
            phaseName = "NEW PHASE";
        }

        string warningMessage =
            $"BOSS ENRAGED - " +
            $"{phaseName.ToUpperInvariant()}!";

        PlayWarning(warningMessage);
    }

    // =========================================================
    // BOSS PRESENTATION
    // =========================================================

    private void RefreshBossPresentation()
    {
        if (bossNameText != null)
        {
            bossNameText.text =
                bossDisplayName;
        }

        RefreshBossHealth();
    }

    private void RefreshBossHealth()
    {
        float currentHealth = 0f;
        float maxHealth = 1f;

        if (activeBoss != null)
        {
            currentHealth =
                Mathf.Max(
                    0f,
                    activeBoss.CurrentHealth
                );

            maxHealth =
                Mathf.Max(
                    1f,
                    activeBoss.MaxHealth
                );
        }

        float normalizedHealth =
            Mathf.Clamp01(
                currentHealth / maxHealth
            );

        if (bossHealthSlider != null)
        {
            bossHealthSlider.value =
                normalizedHealth;
        }

        if (bossHealthText != null)
        {
            bossHealthText.text =
                $"{Mathf.CeilToInt(currentHealth):N0}" +
                $" / " +
                $"{Mathf.CeilToInt(maxHealth):N0}";
        }
    }

    // =========================================================
    // WARNING
    // =========================================================

    private void PlayWarning(
        string message
    )
    {
        StopWarning();

        if (warningText != null)
        {
            warningText.text =
                string.IsNullOrWhiteSpace(message)
                    ? bossDisplayName
                    : message;
        }

        warningCoroutine =
            StartCoroutine(
                WarningRoutine()
            );
    }

    private IEnumerator WarningRoutine()
    {
        SetWarningPanelActive(true);

        if (warningCanvasGroup == null)
        {
            if (warningHoldDuration > 0f)
            {
                yield return
                    new WaitForSecondsRealtime(
                        warningHoldDuration
                    );
            }

            SetWarningPanelActive(false);
            warningCoroutine = null;

            yield break;
        }

        yield return FadeWarning(
            0f,
            1f,
            warningFadeDuration
        );

        if (warningHoldDuration > 0f)
        {
            yield return
                new WaitForSecondsRealtime(
                    warningHoldDuration
                );
        }

        yield return FadeWarning(
            1f,
            0f,
            warningFadeDuration
        );

        SetWarningPanelActive(false);

        warningCoroutine = null;
    }

    private IEnumerator FadeWarning(
        float startAlpha,
        float targetAlpha,
        float duration
    )
    {
        if (warningCanvasGroup == null)
        {
            yield break;
        }

        if (duration <= 0f)
        {
            warningCanvasGroup.alpha =
                targetAlpha;

            yield break;
        }

        float elapsedTime = 0f;

        warningCanvasGroup.alpha =
            startAlpha;

        while (elapsedTime < duration)
        {
            elapsedTime +=
                Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsedTime / duration
                );

            warningCanvasGroup.alpha =
                Mathf.Lerp(
                    startAlpha,
                    targetAlpha,
                    progress
                );

            yield return null;
        }

        warningCanvasGroup.alpha =
            targetAlpha;
    }

    private void StopWarning()
    {
        if (warningCoroutine != null)
        {
            StopCoroutine(
                warningCoroutine
            );

            warningCoroutine = null;
        }

        if (warningCanvasGroup != null)
        {
            warningCanvasGroup.alpha = 0f;
        }

        SetWarningPanelActive(false);
    }

    // =========================================================
    // PANEL VISIBILITY
    // =========================================================

    private void SetWarningPanelActive(
        bool isActive
    )
    {
        if (warningPanel == null)
        {
            return;
        }

        warningPanel.SetActive(isActive);
    }

    private void SetBossHealthPanelActive(
        bool isActive
    )
    {
        if (bossHealthPanel == null)
        {
            return;
        }

        bossHealthPanel.SetActive(isActive);
    }
}