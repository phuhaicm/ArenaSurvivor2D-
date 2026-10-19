using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PracticeVictoryUI : HaiMonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PracticeGameStateController gameStateController;

    [SerializeField]
    private PracticeSceneLoader sceneLoader;

    [Header("Victory UI")]
    [SerializeField]
    private GameObject victoryPanel;

    [SerializeField]
    private Button restartButton;

    [SerializeField]
    private Button mainMenuButton;

    protected override void LoadComponents()
    {
        base.LoadComponents();

        LoadGameStateController();
        LoadSceneLoader();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        LoadGameStateController();
        LoadSceneLoader();

        SubscribeGameStateEvents();
        SubscribeButtonEvents();
    }

    protected override void OnDisable()
    {
        UnsubscribeGameStateEvents();
        UnsubscribeButtonEvents();

        base.OnDisable();
    }

    protected override void Start()
    {
        base.Start();

        RefreshPanelFromCurrentState();
    }

    private void LoadGameStateController()
    {
        if (gameStateController != null)
        {
            return;
        }

        gameStateController =
            FindFirstObjectByType<
                PracticeGameStateController
            >();
    }

    private void LoadSceneLoader()
    {
        if (sceneLoader != null)
        {
            return;
        }

        sceneLoader =
            PracticeSceneLoader.Instance;

        if (sceneLoader != null)
        {
            return;
        }

        sceneLoader =
            FindFirstObjectByType<
                PracticeSceneLoader
            >();
    }

    // =========================================================
    // GAME STATE
    // =========================================================

    private void SubscribeGameStateEvents()
    {
        if (gameStateController == null)
        {
            return;
        }

        gameStateController.StateChanged -=
            HandleGameStateChanged;

        gameStateController.StateChanged +=
            HandleGameStateChanged;
    }

    private void UnsubscribeGameStateEvents()
    {
        if (gameStateController == null)
        {
            return;
        }

        gameStateController.StateChanged -=
            HandleGameStateChanged;
    }

    private void HandleGameStateChanged(
        PracticeGameStates newState
    )
    {
        bool shouldShow =
            newState ==
            PracticeGameStates.Victory;

        SetVictoryPanelActive(
            shouldShow
        );
    }

    private void RefreshPanelFromCurrentState()
    {
        if (gameStateController == null)
        {
            SetVictoryPanelActive(false);
            return;
        }

        HandleGameStateChanged(
            gameStateController.CurrentState
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
                HandleRestartClicked
            );

            restartButton.onClick.AddListener(
                HandleRestartClicked
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
                HandleRestartClicked
            );
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(
                HandleMainMenuClicked
            );
        }
    }

    private void HandleRestartClicked()
    {
        if (sceneLoader == null)
        {
            return;
        }

        string currentSceneName =
            SceneManager.GetActiveScene().name;

        sceneLoader.LoadScene(
            currentSceneName
        );
    }

    private void HandleMainMenuClicked()
    {
        if (sceneLoader == null)
        {
            return;
        }

        sceneLoader.LoadScene(
            PracticeSceneNames.MainMenu
        );
    }

    // =========================================================
    // PANEL
    // =========================================================

    private void SetVictoryPanelActive(
        bool isActive
    )
    {
        if (victoryPanel == null)
        {
            return;
        }

        victoryPanel.SetActive(isActive);
    }
}