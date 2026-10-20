using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PracticeVictoryUI : HaiMonoBehaviour
{
    [Header("Core References")]
    [SerializeField]
    private PracticeGameStateController gameStateController;

    [SerializeField]
    private PracticeSceneLoader sceneLoader;

    [Header("Run Data")]
    [SerializeField]
    private PracticeSurvivalTimer survivalTimer;

    [SerializeField]
    private PracticePlayerKillCounter killCounter;

    [SerializeField]
    private PracticeRunScore runScore;

    [SerializeField]
    private PracticeBestScore bestScore;

    [Header("Victory Panel")]
    [SerializeField]
    private GameObject victoryPanel;

    [Header("Result Text")]
    [SerializeField]
    private TextMeshProUGUI survivalTimeText;

    [SerializeField]
    private TextMeshProUGUI killCountText;

    [SerializeField]
    private TextMeshProUGUI finalScoreText;

    [SerializeField]
    private TextMeshProUGUI bestScoreText;

    [Header("Buttons")]
    [SerializeField]
    private Button restartButton;

    [SerializeField]
    private Button mainMenuButton;

    protected override void LoadComponents()
    {
        base.LoadComponents();

        LoadGameStateController();
        LoadSceneLoader();
        LoadRunSystems();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        LoadGameStateController();
        LoadSceneLoader();
        LoadRunSystems();

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

        RefreshFromCurrentState();
    }

    // =========================================================
    // LOAD REFERENCES
    // =========================================================

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

    private void LoadRunSystems()
    {
        if (survivalTimer == null)
        {
            survivalTimer =
                FindFirstObjectByType<
                    PracticeSurvivalTimer
                >();
        }

        if (killCounter == null)
        {
            killCounter =
                FindFirstObjectByType<
                    PracticePlayerKillCounter
                >();
        }

        if (runScore == null)
        {
            runScore =
                FindFirstObjectByType<
                    PracticeRunScore
                >();
        }

        if (bestScore == null)
        {
            bestScore =
                FindFirstObjectByType<
                    PracticeBestScore
                >();
        }
    }

    // =========================================================
    // GAME STATE EVENTS
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

        if (shouldShow)
        {
            RefreshVictoryResults();
        }

        SetVictoryPanelActive(
            shouldShow
        );
    }

    private void RefreshFromCurrentState()
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
    // RESULT DATA
    // =========================================================

    private void RefreshVictoryResults()
    {
        RefreshSurvivalTime();
        RefreshKillCount();
        RefreshFinalScore();
        RefreshBestScore();
    }

    private void RefreshSurvivalTime()
    {
        if (survivalTimeText == null)
        {
            return;
        }

        float elapsedTime = 0f;

        if (survivalTimer != null)
        {
            elapsedTime =
                survivalTimer.ElapsedTime;
        }

        survivalTimeText.text =
            FormatTime(elapsedTime);
    }

    private void RefreshKillCount()
    {
        if (killCountText == null)
        {
            return;
        }

        int kills = 0;

        if (killCounter != null)
        {
            kills =
                killCounter.CurrentKills;
        }

        killCountText.text =
            kills.ToString("N0");
    }

    private void RefreshFinalScore()
    {
        if (finalScoreText == null)
        {
            return;
        }

        int score = 0;

        if (runScore != null)
        {
            score =
                runScore.FinalScore;
        }

        finalScoreText.text =
            score.ToString("N0");
    }

    private void RefreshBestScore()
    {
        if (bestScoreText == null)
        {
            return;
        }

        int score = 0;

        if (bestScore != null)
        {
            score =
                bestScore.BestScore;
        }

        bestScoreText.text =
            score.ToString("N0");
    }

    private string FormatTime(float totalSeconds)
    {
        int validSeconds =
            Mathf.Max(
                0,
                Mathf.FloorToInt(totalSeconds)
            );

        int minutes =
            validSeconds / 60;

        int seconds =
            validSeconds % 60;

        return $"{minutes:00}:{seconds:00}";
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
            LoadSceneLoader();
        }

        if (sceneLoader == null)
        {
            Debug.LogError(
                $"{name}: Scene Loader đang null.",
                this
            );

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
            LoadSceneLoader();
        }

        if (sceneLoader == null)
        {
            Debug.LogError(
                $"{name}: Scene Loader đang null.",
                this
            );

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