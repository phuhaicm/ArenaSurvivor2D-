using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildStatsPanelController : HaiMonoBehaviour
{
    private PlayerLevelSystem playerLevelSystem;
    private PlayerExperience playerExperience;
    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private PlayerAutoShooter playerAutoShooter;
    private SurvivalTimer survivalTimer;
    private AudioManager audioManager;

    private GameObject panelRootObject;
    private GameObject mainMenuRootObject;
    private GameObject pauseMenuRootObject;
    private GameObject levelUpPopupRootObject;
    private GameObject gameOverRootObject;

    private TextMeshProUGUI statsText;
    private Button[] openButtons;
    private Button closeButton;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadPlayerLevelSystem();
        LoadPlayerExperience();
        LoadPlayerMovement();
        LoadPlayerHealth();
        LoadPlayerAutoShooter();
        LoadSurvivalTimer();
        LoadAudioManager();
        LoadPanelRootObject();
        LoadMainMenuRootObject();
        LoadPauseMenuRootObject();
        LoadLevelUpPopupRootObject();
        LoadGameOverRootObject();
        LoadStatsText();
        LoadOpenButtons();
        LoadCloseButton();
    }

    protected override void Start()
    {
        base.Start();
        HidePanelInstant();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SubscribeButtons();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        UnsubscribeButtons();
    }

    private void Update()
    {
        if (CanReadToggleInput() && Input.GetKeyDown(KeyCode.Tab))
        {
            TogglePanel();
        }

        if (panelRootObject != null && panelRootObject.activeSelf)
        {
            RefreshUI();
        }
    }

    private void LoadPlayerLevelSystem()
    {
        if (playerLevelSystem != null) return;
        playerLevelSystem = FindFirstObjectByType<PlayerLevelSystem>();
    }

    private void LoadPlayerExperience()
    {
        if (playerExperience != null) return;
        playerExperience = FindFirstObjectByType<PlayerExperience>();
    }

    private void LoadPlayerMovement()
    {
        if (playerMovement != null) return;
        playerMovement = FindFirstObjectByType<PlayerMovement>();
    }

    private void LoadPlayerHealth()
    {
        if (playerHealth != null) return;
        playerHealth = FindFirstObjectByType<PlayerHealth>();
    }

    private void LoadPlayerAutoShooter()
    {
        if (playerAutoShooter != null) return;
        playerAutoShooter = FindFirstObjectByType<PlayerAutoShooter>();
    }

    private void LoadSurvivalTimer()
    {
        if (survivalTimer != null) return;
        survivalTimer = FindFirstObjectByType<SurvivalTimer>();
    }

    private void LoadAudioManager()
    {
        if (audioManager != null) return;
        audioManager = FindFirstObjectByType<AudioManager>();
    }

    private void LoadPanelRootObject()
    {
        if (panelRootObject != null) return;

        BuildStatsPanelRoot marker = UIRootLookup.FindRootInCanvas<BuildStatsPanelRoot>(this);
        if (marker == null) return;

        panelRootObject = marker.gameObject;
    }

    private void LoadMainMenuRootObject()
    {
        if (mainMenuRootObject != null) return;

        MainMenuRoot marker = UIRootLookup.FindRootInCanvas<MainMenuRoot>(this);
        if (marker == null) return;

        mainMenuRootObject = marker.gameObject;
    }

    private void LoadPauseMenuRootObject()
    {
        if (pauseMenuRootObject != null) return;

        PauseMenuRoot marker = UIRootLookup.FindRootInCanvas<PauseMenuRoot>(this);
        if (marker == null) return;

        pauseMenuRootObject = marker.gameObject;
    }

    private void LoadLevelUpPopupRootObject()
    {
        if (levelUpPopupRootObject != null) return;

        LevelUpPopupRoot marker = UIRootLookup.FindRootInCanvas<LevelUpPopupRoot>(this);
        if (marker == null) return;

        levelUpPopupRootObject = marker.gameObject;
    }

    private void LoadGameOverRootObject()
    {
        if (gameOverRootObject != null) return;

        GameOverRoot marker = UIRootLookup.FindRootInCanvas<GameOverRoot>(this);
        if (marker == null) return;

        gameOverRootObject = marker.gameObject;
    }

    private void LoadStatsText()
    {
        if (statsText != null) return;

        BuildStatsDisplayTextUI marker = UIRootLookup.FindInRoot<BuildStatsPanelRoot, BuildStatsDisplayTextUI>(this);
        if (marker == null) return;

        statsText = marker.GetComponent<TextMeshProUGUI>();
    }

    private void LoadOpenButtons()
    {
        if (openButtons != null && openButtons.Length > 0) return;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        BuildStatsOpenButtonUI[] markers = canvas.GetComponentsInChildren<BuildStatsOpenButtonUI>(true);
        if (markers == null || markers.Length == 0) return;

        openButtons = new Button[markers.Length];

        for (int i = 0; i < markers.Length; i++)
        {
            openButtons[i] = markers[i].GetComponent<Button>();
        }
    }

    private void LoadCloseButton()
    {
        if (closeButton != null) return;

        BuildStatsCloseButtonUI marker = UIRootLookup.FindInRoot<BuildStatsPanelRoot, BuildStatsCloseButtonUI>(this);
        if (marker == null) return;

        closeButton = marker.GetComponent<Button>();
    }

    private void SubscribeButtons()
    {
        if (openButtons != null)
        {
            for (int i = 0; i < openButtons.Length; i++)
            {
                if (openButtons[i] == null) continue;

                openButtons[i].onClick.RemoveListener(HandleOpenClicked);
                openButtons[i].onClick.AddListener(HandleOpenClicked);
            }
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(HandleCloseClicked);
            closeButton.onClick.AddListener(HandleCloseClicked);
        }
    }

    private void UnsubscribeButtons()
    {
        if (openButtons != null)
        {
            for (int i = 0; i < openButtons.Length; i++)
            {
                if (openButtons[i] == null) continue;
                openButtons[i].onClick.RemoveListener(HandleOpenClicked);
            }
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(HandleCloseClicked);
        }
    }

    private bool CanReadToggleInput()
    {
        if (mainMenuRootObject != null && mainMenuRootObject.activeSelf) return false;
        if (pauseMenuRootObject != null && pauseMenuRootObject.activeSelf) return false;
        if (levelUpPopupRootObject != null && levelUpPopupRootObject.activeSelf) return false;
        if (gameOverRootObject != null && gameOverRootObject.activeSelf) return false;
        return true;
    }

    private void HandleOpenClicked()
    {
        audioManager?.PlaySfx(AudioCueKey.ButtonClick);
        ShowPanel();
    }

    private void HandleCloseClicked()
    {
        audioManager?.PlaySfx(AudioCueKey.ButtonClick);
        HidePanel();
    }

    private void TogglePanel()
    {
        if (panelRootObject == null) return;

        if (panelRootObject.activeSelf)
        {
            HidePanel();
            return;
        }

        ShowPanel();
    }

    private void ShowPanel()
    {
        if (panelRootObject != null)
        {
            panelRootObject.SetActive(true);
            panelRootObject.transform.SetAsLastSibling();
        }

        RefreshUI();
    }

    private void HidePanel()
    {
        if (panelRootObject != null)
        {
            panelRootObject.SetActive(false);
        }
    }

    private void HidePanelInstant()
    {
        HidePanel();
    }

    private void RefreshUI()
    {
        if (statsText == null) return;

        int level = playerLevelSystem != null ? playerLevelSystem.CurrentLevel : 1;
        int totalXP = playerExperience != null ? playerExperience.TotalExperience : 0;
        float moveSpeed = playerMovement != null ? playerMovement.MoveSpeed : 0f;
        int currentHealth = playerHealth != null ? playerHealth.CurrentHealth : 0;
        int maxHealth = playerHealth != null ? playerHealth.MaxHealth : 0;
        int bulletDamage = playerAutoShooter != null ? playerAutoShooter.BulletDamage : 0;
        float shootInterval = playerAutoShooter != null ? playerAutoShooter.ShootInterval : 0f;
        int projectileCount = playerAutoShooter != null ? playerAutoShooter.ProjectileCount : 0;
        float survivalTime = survivalTimer != null ? survivalTimer.ElapsedTime : 0f;

        statsText.text =
            $"Level: {level}\n" +
            $"Total XP: {totalXP}\n" +
            $"Move Speed: {moveSpeed:0.00}\n" +
            $"Health: {currentHealth} / {maxHealth}\n" +
            $"Bullet Damage: {bulletDamage}\n" +
            $"Fire Rate: {BuildStatsFormatter.FormatFireRate(shootInterval)}\n" +
            $"Projectile Count: {projectileCount}\n" +
            $"Survival Time: {BuildStatsFormatter.FormatTime(survivalTime)}";
    }
}