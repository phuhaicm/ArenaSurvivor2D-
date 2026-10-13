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

    [Header("Game Over UI")]
    [SerializeField]
    private GameObject gameOverPanel;

    [SerializeField]
    private Button restartButton;

    [SerializeField]
    private Button quitButton;

    private bool isGameOverHandled;

    protected override void LoadComponents()
    {
        base.LoadComponents();

        LoadPlayerHealth();
        LoadGameStateController();
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

    protected override void OnEnable()
    {
        base.OnEnable();

        LoadPlayerHealth();
        LoadGameStateController();

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

        //ShowGameOverPanel();

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
        bool shoulGameOverPanelMenu = gameStates == PracticeGameStates.Paused;

        SetGameOverPanelMenu(shoulGameOverPanelMenu);
    }
    protected void SetGameOverPanelMenu(bool isPause)
    {
        if(gameOverPanel == null) return;

        gameOverPanel.SetActive(isPause);
    }

    private void HandleRestartButtonClicked()
    {
        if (!isGameOverHandled)
        {
            return;
        }

        Time.timeScale = 1f;

        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(
            currentScene.buildIndex
        );
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