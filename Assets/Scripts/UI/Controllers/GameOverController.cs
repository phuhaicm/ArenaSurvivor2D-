using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverController : HaiMonoBehaviour
{
    private PlayerHealth playerHealth;
    private PlayerLevelSystem playerLevelSystem;
    private PlayerExperience playerExperience;
    private SurvivalTimer survivalTimer;
    private GamePauseController gamePauseController;
    private AudioManager audioManager;

    private GameOverRoot gameOverRoot;
    private GameObject gameOverRootObject;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI currentRunStatsText;
    private TextMeshProUGUI bestRunStatsText;
    private Button retryButton;
    private Button menuButton;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadPlayerHealth();
        LoadPlayerLevelSystem();
        LoadPlayerExperience();
        LoadSurvivalTimer();
        LoadGamePauseController();
        LoadAudioManager();
        LoadGameOverRoot();
        LoadGameOverRootObject();
        LoadTitleText();
        LoadCurrentRunStatsText();
        LoadBestRunStatsText();
        LoadRetryButton();
        LoadMenuButton();
    }

    protected override void Start()
    {
        base.Start();
        HideGameOverInstant();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SubscribeEvents();
        SubscribeButtons();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        UnsubscribeEvents();
        UnsubscribeButtons();
    }

    private void LoadPlayerHealth()
    {
        if (playerHealth != null) return;
        playerHealth = FindFirstObjectByType<PlayerHealth>();
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

    private void LoadSurvivalTimer()
    {
        if (survivalTimer != null) return;
        survivalTimer = FindFirstObjectByType<SurvivalTimer>();
    }

    private void LoadGamePauseController()
    {
        if (gamePauseController != null) return;
        gamePauseController = FindFirstObjectByType<GamePauseController>();
    }

    private void LoadAudioManager()
    {
        if (audioManager != null) return;
        audioManager = FindFirstObjectByType<AudioManager>();
    }

    private void LoadGameOverRoot()
    {
        if (gameOverRoot != null) return;
        gameOverRoot = UIRootLookup.FindRootInCanvas<GameOverRoot>(this);
    }

    private void LoadGameOverRootObject()
    {
        if (gameOverRootObject != null) return;
        if (gameOverRoot == null) return;

        gameOverRootObject = gameOverRoot.gameObject;
    }

    private void LoadTitleText()
    {
        if (titleText != null) return;
        if (gameOverRoot == null) return;

        GameOverTitleUI marker = gameOverRoot.GetComponentInChildren<GameOverTitleUI>(true);
        if (marker == null) return;

        titleText = marker.GetComponent<TextMeshProUGUI>();
    }

    private void LoadCurrentRunStatsText()
    {
        if (currentRunStatsText != null) return;
        if (gameOverRoot == null) return;

        CurrentRunStatsTextUI marker = gameOverRoot.GetComponentInChildren<CurrentRunStatsTextUI>(true);
        if (marker == null) return;

        currentRunStatsText = marker.GetComponent<TextMeshProUGUI>();
    }

    private void LoadBestRunStatsText()
    {
        if (bestRunStatsText != null) return;
        if (gameOverRoot == null) return;

        BestRunStatsTextUI marker = gameOverRoot.GetComponentInChildren<BestRunStatsTextUI>(true);
        if (marker == null) return;

        bestRunStatsText = marker.GetComponent<TextMeshProUGUI>();
    }

    private void LoadRetryButton()
    {
        if (retryButton != null) return;
        if (gameOverRoot == null) return;

        GameOverRetryButtonUI marker = gameOverRoot.GetComponentInChildren<GameOverRetryButtonUI>(true);
        if (marker == null) return;

        retryButton = marker.GetComponent<Button>();
    }

    private void LoadMenuButton()
    {
        if (menuButton != null) return;
        if (gameOverRoot == null) return;

        GameOverMenuButtonUI marker = gameOverRoot.GetComponentInChildren<GameOverMenuButtonUI>(true);
        if (marker == null) return;

        menuButton = marker.GetComponent<Button>();
    }

    private void SubscribeEvents()
    {
        if (playerHealth != null)
        {
            playerHealth.Died += HandlePlayerDied;
        }
    }

    private void UnsubscribeEvents()
    {
        if (playerHealth != null)
        {
            playerHealth.Died -= HandlePlayerDied;
        }
    }

    private void SubscribeButtons()
    {
        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(HandleRetryClicked);
            retryButton.onClick.AddListener(HandleRetryClicked);
        }

        if (menuButton != null)
        {
            menuButton.onClick.RemoveListener(HandleMenuClicked);
            menuButton.onClick.AddListener(HandleMenuClicked);
        }
    }

    private void UnsubscribeButtons()
    {
        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(HandleRetryClicked);
        }

        if (menuButton != null)
        {
            menuButton.onClick.RemoveListener(HandleMenuClicked);
        }
    }

    private void HandlePlayerDied()
    {
        audioManager?.PlaySfx(AudioCueKey.GameOver);
        SaveBestRunStats();
        ShowGameOver();
        PauseGame();
    }

    private void HandleRetryClicked()
    {
        audioManager?.PlaySfx(AudioCueKey.ButtonClick);
        GameBootFlow.StartIntoGameplay = true;
        ReloadCurrentScene();
    }

    private void HandleMenuClicked()
    {
        audioManager?.PlaySfx(AudioCueKey.ButtonClick);
        GameBootFlow.StartIntoGameplay = false;
        ReloadCurrentScene();
    }

    private void ShowGameOver()
    {
        if (gameOverRootObject != null)
        {
            gameOverRootObject.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = "GAME OVER";
        }

        RefreshSummaryTexts();
    }

    private void RefreshSummaryTexts()
    {
        int level = playerLevelSystem != null ? playerLevelSystem.CurrentLevel : 1;
        int totalXP = playerExperience != null ? playerExperience.TotalExperience : 0;
        float time = survivalTimer != null ? survivalTimer.ElapsedTime : 0f;

        if (currentRunStatsText != null)
        {
            currentRunStatsText.text =
                "CURRENT RUN\n" +
                RunSummaryFormatter.FormatRunStats(level, totalXP, time);
        }

        if (bestRunStatsText != null)
        {
            bestRunStatsText.text =
                "BEST RUN\n" +
                RunSummaryFormatter.FormatBestStats();
        }
    }

    private void SaveBestRunStats()
    {
        int level = playerLevelSystem != null ? playerLevelSystem.CurrentLevel : 1;
        int totalXP = playerExperience != null ? playerExperience.TotalExperience : 0;
        float time = survivalTimer != null ? survivalTimer.ElapsedTime : 0f;

        BestRunStats.SaveIfBetter(time, level, totalXP);
    }

    private void HideGameOverInstant()
    {
        if (gameOverRootObject != null)
        {
            gameOverRootObject.SetActive(false);
        }
    }

    private void PauseGame()
    {
        gamePauseController?.PauseGame();
    }

    private void ReloadCurrentScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}