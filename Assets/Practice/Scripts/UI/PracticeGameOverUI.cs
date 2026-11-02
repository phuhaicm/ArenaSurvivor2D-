using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PracticeGameOverUI : HaiMonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PracticePlayerHealth playerHealth;

    [SerializeField]
    private PracticeGameStateController gameStateController;

    [Header("Run Data References")]
    [SerializeField]
    private PracticeSurvivalTimer survivalTimer;

    [SerializeField]
    private PracticePlayerKillCounter killCounter;

    [SerializeField]
    private PracticeRunScore runScore;

    [SerializeField]
    private PracticeBestScore bestScore;

    [Header("Game Over UI")]
    [SerializeField]
    private GameObject gameOverPanel;

    [Header("Result Texts (Auto-Resolved from Panel)")]
    [SerializeField]
    private TextMeshProUGUI finalTimeText;

    [SerializeField]
    private TextMeshProUGUI finalKillCountText;

    [SerializeField]
    private TextMeshProUGUI finalScoreText;

    [Header("Scene Loading & Buttons")]
    [SerializeField]
    protected PracticeSceneLoader sceneLoader;

    [SerializeField]
    private Button restartButton;

    [SerializeField]
    private Button quitButton;

    [SerializeField]
    private Button mainMenuButton;

    private bool isGameOverHandled;

    protected override void LoadComponents()
    {
        base.LoadComponents();

        LoadPlayerHealth();
        LoadGameStateController();
        LoadSceneLoader();
        LoadRunSystems();
        LoadResultTextsFromPanel();
    }

    protected void LoadSceneLoader()
    {
        if (sceneLoader != null) return;

        sceneLoader = PracticeSceneLoader.Instance;

        if (sceneLoader != null) return;
        sceneLoader = FindFirstObjectByType<PracticeSceneLoader>();
    }

    private void LoadPlayerHealth()
    {
        if (playerHealth != null)
        {
            return;
        }

        playerHealth =
            FindFirstObjectByType<PracticePlayerHealth>();
    }

    private void LoadGameStateController()
    {
        if (gameStateController != null)
        {
            return;
        }

        gameStateController =
            FindFirstObjectByType<PracticeGameStateController>();
    }

    private void LoadRunSystems()
    {
        if (survivalTimer == null)
        {
            survivalTimer = FindFirstObjectByType<PracticeSurvivalTimer>();
        }

        if (killCounter == null)
        {
            killCounter = FindFirstObjectByType<PracticePlayerKillCounter>();
        }

        if (runScore == null)
        {
            runScore = FindFirstObjectByType<PracticeRunScore>();
        }

        if (bestScore == null)
        {
            bestScore = FindFirstObjectByType<PracticeBestScore>();
        }
    }

    private void LoadResultTextsFromPanel()
    {
        if (gameOverPanel == null)
        {
            return;
        }

        if (finalTimeText == null)
        {
            Transform timeTr = gameOverPanel.transform.Find("FinalTimeText");
            if (timeTr != null)
            {
                finalTimeText = timeTr.GetComponent<TextMeshProUGUI>();
            }
        }

        if (finalKillCountText == null)
        {
            Transform killTr = gameOverPanel.transform.Find("FinalKillCountText");
            if (killTr != null)
            {
                finalKillCountText = killTr.GetComponent<TextMeshProUGUI>();
            }
        }

        if (finalScoreText == null)
        {
            Transform scoreTr = gameOverPanel.transform.Find("FinalScoreText");
            if (scoreTr != null)
            {
                finalScoreText = scoreTr.GetComponent<TextMeshProUGUI>();
            }
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        LoadPlayerHealth();
        LoadGameStateController();
        LoadSceneLoader();
        LoadRunSystems();
        LoadResultTextsFromPanel();

        SubscribePlayerEvents();
        SubscribeButtonEvents();
        SubscribeGameStateEvents();
    }

    protected override void OnDisable()
    {
        UnsubscribePlayerEvents();
        UnsubscribeButtonEvents();
        UnSubscribeGameStateEvents();

        base.OnDisable();
    }

    protected override void Start()
    {
        base.Start();

        isGameOverHandled = false;
        HideGameOverPanel();
    }

    // =========================================================
    // PLAYER EVENT
    // =========================================================

    private void SubscribePlayerEvents()
    {
        if (playerHealth == null)
        {
            Debug.LogError(
                $"{name}: Player Health đang null.",
                this
            );

            return;
        }

        playerHealth.Died -= HandlePlayerDied;
        playerHealth.Died += HandlePlayerDied;
    }

    private void UnsubscribePlayerEvents()
    {
        if (playerHealth == null)
        {
            return;
        }

        playerHealth.Died -= HandlePlayerDied;
    }

    private void HandlePlayerDied()
    {
        if (isGameOverHandled)
        {
            return;
        }

        if (gameStateController == null)
        {
            Debug.LogError(
                $"{name}: Game State Controller đang null.",
                this
            );

            return;
        }

        bool gameOverStarted =
            gameStateController.RequestGameOver();

        if (!gameOverStarted &&
            !gameStateController.IsGameOver)
        {
            return;
        }

        isGameOverHandled = true;

        Debug.Log(
            $"{name}: Game Over started.",
            this
        );
    }

    // =========================================================
    // BUTTON EVENTS
    // =========================================================

    private void SubscribeButtonEvents()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(
                HandleRestartButtonClicked
            );

            restartButton.onClick.AddListener(
                HandleRestartButtonClicked
            );
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(
                HandleQuitButtonClicked
            );

            quitButton.onClick.AddListener(
                HandleQuitButtonClicked
            );
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(
                HandleMainMenuClicked
            );

            mainMenuButton.onClick.AddListener(
                HandleMainMenuClicked
            );
        }
    }

    private void UnsubscribeButtonEvents()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(
                HandleRestartButtonClicked
            );
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(
                HandleQuitButtonClicked
            );
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(
                HandleMainMenuClicked
            );
        }
    }

    protected void SubscribeGameStateEvents()
    {
        if (gameStateController == null) return;

        gameStateController.StateChanged -= HandleGameStateChanged;
        gameStateController.StateChanged += HandleGameStateChanged;
        HandleGameStateChanged(gameStateController.CurrentState);
    }

    protected void UnSubscribeGameStateEvents()
    {
        if (gameStateController == null) return;

        gameStateController.StateChanged -= HandleGameStateChanged;
    }

    protected void HandleGameStateChanged(PracticeGameStates gameStates)
    {
        // SỬA LỖI: Chỉ hiện GameOverPanel khi trạng thái là GameOver (không phải Paused)
        bool shouldShowGameOverPanel = gameStates == PracticeGameStates.GameOver;

        if (shouldShowGameOverPanel)
        {
            isGameOverHandled = true;
            RefreshGameOverResultTexts();
        }

        SetGameOverPanelMenu(shouldShowGameOverPanel);
    }

    private void RefreshGameOverResultTexts()
    {
        LoadRunSystems();
        LoadResultTextsFromPanel();

        if (runScore != null)
        {
            runScore.CalcutedScore();
        }

        if (bestScore != null && runScore != null)
        {
            bestScore.EvaluateScore(runScore.FinalScore);
        }

        if (finalTimeText != null)
        {
            float elapsed = survivalTimer != null ? survivalTimer.ElapsedTime : 0f;
            int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(elapsed));
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            finalTimeText.text = $"Time: {minutes:00}:{seconds:00}";
        }

        if (finalKillCountText != null)
        {
            int kills = killCounter != null ? killCounter.CurrentKills : 0;
            finalKillCountText.text = $"Kills: {kills:N0}";
        }

        if (finalScoreText != null)
        {
            int score = runScore != null ? runScore.FinalScore : 0;
            finalScoreText.text = $"Score: {score:N0}";
        }
    }

    protected void SetGameOverPanelMenu(bool isActive)
    {
        if (gameOverPanel == null) return;

        gameOverPanel.SetActive(isActive);
    }

    protected void HandleMainMenuClicked()
    {
        if (sceneLoader == null)
        {
            LoadSceneLoader();
        }

        // SỬA LỖI: Chuyển về Scene MainMenu thay vì tải lại Scene hiện tại
        if (sceneLoader != null)
        {
            sceneLoader.LoadScene(PracticeSceneNames.MainMenu);
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(PracticeSceneNames.MainMenu);
    }

    private void HandleRestartButtonClicked()
    {
        if (sceneLoader == null)
        {
            LoadSceneLoader();
        }

        string currentSceneName = SceneManager.GetActiveScene().name;

        if (sceneLoader != null)
        {
            sceneLoader.LoadScene(currentSceneName);
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(currentSceneName);
    }

    private void HandleQuitButtonClicked()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

    // =========================================================
    // GAME OVER PANEL
    // =========================================================

    private void ShowGameOverPanel()
    {
        if (gameOverPanel == null)
        {
            return;
        }

        gameOverPanel.SetActive(true);
    }

    private void HideGameOverPanel()
    {
        if (gameOverPanel == null)
        {
            return;
        }

        gameOverPanel.SetActive(false);
    }
}