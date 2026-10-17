using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PracticePauseMenuUI : HaiMonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PracticeGameStateController gameStateController;

    [Header("Pause UI")]
    [SerializeField]
    private GameObject pausePanel;
    [Header("Scene Loading")]
    [SerializeField] protected PracticeSceneLoader sceneLoader;

    [SerializeField]
    private Button resumeButton;
    [SerializeField]
    private Button mainMenuButton;

    [SerializeField]
    private Button restartButton;

    [SerializeField]
    private Button quitButton;

    protected override void LoadComponents()
    {
        base.LoadComponents();

        LoadSceneLoader();
        LoadGameStateController();
    }
    protected void LoadSceneLoader()
    {
        if(sceneLoader != null) return;

        sceneLoader = PracticeSceneLoader.Instance;

        if(sceneLoader != null) return;
        sceneLoader = FindFirstObjectByType<PracticeSceneLoader>();
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

    protected override void OnEnable()
    {
        base.OnEnable();

        LoadGameStateController();
        SubscribeButtonEvents();
        SubscribeGameStateEvents();
    }

    protected override void OnDisable()
    {
        UnsubscribeButtonEvents();
        UnSubscribeGameStateEvents();
        base.OnDisable();
    }

    protected override void Start()
    {
        base.Start();

        if(gameStateController == null)
        {
            SetPausePanelMenu(false);
            return;
        }
        HandleGameStateChanged(gameStateController.CurrentState);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    // =========================================================
    // PAUSE FLOW
    // =========================================================

    private void TogglePause()
    {
        if (gameStateController == null)
        {
            return;
        }

        // Game Over là trạng thái kết thúc.
        if (gameStateController.IsGameOver)
        {
            return;
        }

        // ESC lần hai dùng để Resume.
        if (gameStateController.IsPaused)
        {
            ResumeGame();
            return;
        }

        // Chỉ được Pause từ Running.
        // Nếu đang UpgradeChoice thì không can thiệp.
        if (!gameStateController.IsRunning)
        {
            return;
        }

        PauseGame();
    }
    private void PauseGame()
    {
        if (gameStateController == null)
        {
            return;
        }

        bool pausedSuccessfully =
            gameStateController.RequestPaused();

        if (!pausedSuccessfully)
        {
            return;
        }

        //ShowPausePanel();
    }

    private void ResumeGame()
    {
        if (gameStateController == null)
        {
            return;
        }

        if (!gameStateController.IsPaused)
        {
            return;
        }

        bool resumedSuccessfully =
            gameStateController.RequestRunning();

        if (!resumedSuccessfully)
        {
            return;
        }

        //HidePausePanel();
    }

    // =========================================================
    // BUTTON EVENTS
    // =========================================================

    private void SubscribeButtonEvents()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(
                HandleResumeButtonClicked
            );

            resumeButton.onClick.AddListener(
                HandleResumeButtonClicked
            );
        }

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
        if(mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(HandleMainMenuClicked);
        }
    }
    protected void SubscribeGameStateEvents()
    {
        if(gameStateController == null) return;

        gameStateController.StateChanged += HandleGameStateChanged;
        HandleGameStateChanged(gameStateController.CurrentState);
    }
    protected void UnSubscribeGameStateEvents()
    {
        if(gameStateController == null) return;

        gameStateController.StateChanged -= HandleGameStateChanged;
    }
    protected void HandleGameStateChanged(PracticeGameStates gameStates)
    {
        bool shoulPausePanelMenu = gameStates == PracticeGameStates.Paused;

        SetPausePanelMenu(shoulPausePanelMenu);
    }
    protected void SetPausePanelMenu(bool isPause)
    {
        if(pausePanel == null) return;

        pausePanel.SetActive(isPause);
    }

    private void UnsubscribeButtonEvents()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(
                HandleResumeButtonClicked
            );
        }

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
        if(mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(HandleMainMenuClicked);
        }
    }
    protected void HandleMainMenuClicked()
    {
        PracticeAudioManager.Instance?.PlayButtonClickSfx();

        if(sceneLoader == null)
        {
            LoadSceneLoader();
            return;
        }
        sceneLoader.LoadScene(PracticeSceneNames.MainMenu);//UI chỉ gửi yêu cầu tải main menu
    }

    private void HandleResumeButtonClicked()
    {
        PracticeAudioManager.Instance?.PlayButtonClickSfx();
        ResumeGame();
    }

    private void HandleRestartButtonClicked()
    {
        PracticeAudioManager.Instance?.PlayButtonClickSfx();
        if(sceneLoader == null)
        {
            LoadSceneLoader();
            return;
        }
        
        string currentSceneName = SceneManager.GetActiveScene().name;//dòng này lấy scene hiện tại
        sceneLoader.LoadScene(currentSceneName);
    }

    private void HandleQuitButtonClicked()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

    // =========================================================
    // PAUSE PANEL
    // =========================================================

    private void ShowPausePanel()
    {
        if (pausePanel == null)
        {
            return;
        }

        pausePanel.SetActive(true);
    }

    private void HidePausePanel()
    {
        if (pausePanel == null)
        {
            return;
        }

        pausePanel.SetActive(false);
    }
}