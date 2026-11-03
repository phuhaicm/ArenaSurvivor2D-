using TMPro;
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
    [SerializeField]
    protected PracticeSceneLoader sceneLoader;

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
        if (sceneLoader != null) return;

        sceneLoader = PracticeSceneLoader.Instance;

        if (sceneLoader != null) return;
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
        LoadSceneLoader();
        SanitizeAllButtons();
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

        if (gameStateController == null)
        {
            SetPausePanelMenu(false);
            return;
        }
        HandleGameStateChanged(gameStateController.CurrentState);
    }

    private void SanitizeAllButtons()
    {
        SanitizeButtonChildren(resumeButton);
        SanitizeButtonChildren(mainMenuButton);
        SanitizeButtonChildren(restartButton);
        SanitizeButtonChildren(quitButton);
    }

    private void SanitizeButtonChildren(Button parentButton)
    {
        if (parentButton == null) return;

        Button[] childButtons = parentButton.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < childButtons.Length; i++)
        {
            if (childButtons[i] != null && childButtons[i] != parentButton)
            {
                childButtons[i].enabled = false;
            }
        }

        TMP_Text[] childTexts = parentButton.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < childTexts.Length; i++)
        {
            if (childTexts[i] != null)
            {
                childTexts[i].raycastTarget = false;
            }
        }
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

        if (gameStateController.IsGameOver || gameStateController.IsVictory)
        {
            return;
        }

        if (gameStateController.IsPaused)
        {
            ResumeGame();
            return;
        }

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

        gameStateController.RequestPaused();
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

        gameStateController.RequestRunning();
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
        bool shouldShowPausePanel = gameStates == PracticeGameStates.Paused;

        SetPausePanelMenu(shouldShowPausePanel);
    }

    protected void SetPausePanelMenu(bool isPause)
    {
        if (pausePanel == null) return;

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

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(HandleMainMenuClicked);
        }
    }

    protected void HandleMainMenuClicked()
    {
        PracticeAudioManager.Instance?.PlayButtonClickSfx();

        if (sceneLoader == null)
        {
            LoadSceneLoader();
        }

        Time.timeScale = 1f;

        if (sceneLoader != null && sceneLoader.LoadScene(PracticeSceneNames.MainMenu))
        {
            return;
        }

        SceneManager.LoadScene(PracticeSceneNames.MainMenu);
    }

    private void HandleResumeButtonClicked()
    {
        PracticeAudioManager.Instance?.PlayButtonClickSfx();
        ResumeGame();
    }

    private void HandleRestartButtonClicked()
    {
        PracticeAudioManager.Instance?.PlayButtonClickSfx();

        if (sceneLoader == null)
        {
            LoadSceneLoader();
        }

        string currentSceneName = SceneManager.GetActiveScene().name;
        Time.timeScale = 1f;

        if (sceneLoader != null && sceneLoader.LoadScene(currentSceneName))
        {
            return;
        }

        SceneManager.LoadScene(currentSceneName);
    }

    private void HandleQuitButtonClicked()
    {
        PracticeAudioManager.Instance?.PlayButtonClickSfx();
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
