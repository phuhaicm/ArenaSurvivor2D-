using System.Collections;//cần cho ienumerator
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PracticeBossPresentationUI: HaiMonoBehaviour
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
    private Coroutine warningCoroutine;

    protected override void LoadComponents()
    {
        base.LoadComponents();

        LoadBossController();
    }

    protected override void OnEnable()// cái này là để bật
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

    protected override void OnDisable()// ngừng nghe
    {
        UnsubscribeBossControllerEvents();
        UnsubscribeActiveBossEvents();
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
        SetBossHealthPanelActive(false);//boss chưa spawn thì ko hiện health bar

        if (warningText != null)
        {
            warningText.text =
                "FINAL BOSS";
        }

        if (warningCanvasGroup != null)
        {
            warningCanvasGroup.alpha = 0f;// vô hình
            warningCanvasGroup.interactable = false;// ko nhận click
            warningCanvasGroup.blocksRaycasts = false;// ko chặn UI phía dưới
        }

        SetWarningPanelActive(false);//tắt panel hoàn toàn

        ConfigureHealthSlider();//đưa silder về cấu hình hiển thị health chuẩn hóa 0-1
    }

    private void ConfigureHealthSlider()
    {
        if (bossHealthSlider == null)
        {
            return;
        }

        bossHealthSlider.minValue = 0f;
        bossHealthSlider.maxValue = 1f;
        bossHealthSlider.wholeNumbers = false;// cho biết giá trị trung gian
        bossHealthSlider.interactable = false;//người chơi ko được kéo máu boss health
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
        AttachActiveBoss();// lấy boss vừa spawn từ controller và đăng ký event boss
        RefreshBossPresentation();//điền tên boss và health hiện tại
        SetBossHealthPanelActive(true);
        PlayWarning();
    }

    // =========================================================
    // ACTIVE BOSS
    // =========================================================

    private void AttachActiveBoss()
    {
        UnsubscribeActiveBossEvents();//gỡ listener boss cũ trước khi chuyển references

        if (bossController == null)
        {
            activeBoss = null;
            return;
        }

        activeBoss =
            bossController.ActiveBoss;

        SubscribeActiveBossEvents();
    }

    private void SubscribeActiveBossEvents()
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

    private void UnsubscribeActiveBossEvents()
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
        UnsubscribeActiveBossEvents();

        activeBoss = null;

        SetBossHealthPanelActive(false);
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
            );// giữ giá trị từ 0-1

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

    private void PlayWarning()
    {
        StopWarning();

        warningCoroutine =
            StartCoroutine(
                WarningRoutine()
            );
    }

    private IEnumerator WarningRoutine()
    {
        SetWarningPanelActive(true);

        if (warningCanvasGroup == null)//ko thể fade nên chờ hold duration và tắt panel
        {
            yield return new WaitForSecondsRealtime(
                warningHoldDuration
            );

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
            yield return new WaitForSecondsRealtime(
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
        if (warningCoroutine == null)
        {
            return;
        }

        StopCoroutine(
            warningCoroutine
        );

        warningCoroutine = null;

        if (warningCanvasGroup != null)
        {
            warningCanvasGroup.alpha = 0f;
        }

        SetWarningPanelActive(false);
    }

    // =========================================================
    // PANEL VISIBILITY
    // =========================================================

    private void SetWarningPanelActive (bool isActive)
    {
        if (warningPanel == null)
        {
            return;
        }

        warningPanel.SetActive(isActive);
    }

    private void SetBossHealthPanelActive(bool isActive)
    {
        if (bossHealthPanel == null)
        {
            return;
        }

        bossHealthPanel.SetActive(isActive);
    }
}