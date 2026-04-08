using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : HaiMonoBehaviour
{
    private GamePauseController gamePauseController;
    private AudioManager audioManager;

    private GameObject pauseMenuRootObject;
    private GameObject mainMenuRootObject;
    private GameObject levelUpPopupRootObject;
    private GameObject gameOverRootObject;
    private GameObject settingsRootObject;

    private Button resumeButton;
    private Button menuButton;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadGamePauseController();
        LoadAudioManager();
        LoadPauseMenuRootObject();
        LoadMainMenuRootObject();
        LoadLevelUpPopupRootObject();
        LoadGameOverRootObject();
        LoadSettingsRootObject();
        LoadResumeButton();
        LoadMenuButton();
    }

    protected override void Start()
    {
        base.Start();
        HidePauseMenuInstant();
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
        if (!CanReadPauseInput()) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
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

    private void LoadPauseMenuRootObject()
    {
        if (pauseMenuRootObject != null) return;

        PauseMenuRoot marker = UIRootLookup.FindRootInCanvas<PauseMenuRoot>(this);
        if (marker == null) return;

        pauseMenuRootObject = marker.gameObject;
    }

    private void LoadMainMenuRootObject()
    {
        if (mainMenuRootObject != null) return;

        MainMenuRoot marker = UIRootLookup.FindRootInCanvas<MainMenuRoot>(this);
        if (marker == null) return;

        mainMenuRootObject = marker.gameObject;
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

    private void LoadSettingsRootObject()
    {
        if (settingsRootObject != null) return;

        AudioSettingsPanelRoot marker = UIRootLookup.FindRootInCanvas<AudioSettingsPanelRoot>(this);
        if (marker == null) return;

        settingsRootObject = marker.gameObject;
    }

    private void LoadResumeButton()
    {
        if (resumeButton != null) return;

        PauseMenuResumeButtonUI marker = UIRootLookup.FindInRoot<PauseMenuRoot, PauseMenuResumeButtonUI>(this);
        if (marker == null) return;

        resumeButton = marker.GetComponent<Button>();
    }

    private void LoadMenuButton()
    {
        if (menuButton != null) return;

        PauseMenuBackToMenuButtonUI marker = UIRootLookup.FindInRoot<PauseMenuRoot, PauseMenuBackToMenuButtonUI>(this);
        if (marker == null) return;

        menuButton = marker.GetComponent<Button>();
    }

    private void SubscribeButtons()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(HandleResumeClicked);
            resumeButton.onClick.AddListener(HandleResumeClicked);
        }

        if (menuButton != null)
        {
            menuButton.onClick.RemoveListener(HandleMenuClicked);
            menuButton.onClick.AddListener(HandleMenuClicked);
        }
    }

    private void UnsubscribeButtons()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(HandleResumeClicked);
        }

        if (menuButton != null)
        {
            menuButton.onClick.RemoveListener(HandleMenuClicked);
        }
    }

    private bool CanReadPauseInput()
    {
        if (mainMenuRootObject != null && mainMenuRootObject.activeSelf) return false;
        if (levelUpPopupRootObject != null && levelUpPopupRootObject.activeSelf) return false;
        if (gameOverRootObject != null && gameOverRootObject.activeSelf) return false;
        return true;
    }

    private void TogglePauseMenu()
    {
        if (pauseMenuRootObject == null) return;

        if (pauseMenuRootObject.activeSelf)
        {
            ResumeGameplay();
            return;
        }

        OpenPauseMenu();
    }

    private void HandleResumeClicked()
    {
        audioManager?.PlaySfx(AudioCueKey.ButtonClick);
        ResumeGameplay();
    }

    private void HandleMenuClicked()
    {
        audioManager?.PlaySfx(AudioCueKey.ButtonClick);
        GameBootFlow.StartIntoGameplay = false;
        ReloadCurrentScene();
    }

    private void OpenPauseMenu()
    {
        if (pauseMenuRootObject != null)
        {
            pauseMenuRootObject.SetActive(true);
        }

        gamePauseController?.PauseGame();
    }

    private void ResumeGameplay()
    {
        HideSettingsOverlay();
        HidePauseMenu();
        gamePauseController?.ResumeGame();
    }

    private void HidePauseMenu()
    {
        if (pauseMenuRootObject != null)
        {
            pauseMenuRootObject.SetActive(false);
        }
    }

    private void HidePauseMenuInstant()
    {
        HidePauseMenu();
    }

    private void HideSettingsOverlay()
    {
        if (settingsRootObject != null)
        {
            settingsRootObject.SetActive(false);
        }
    }

    private void ReloadCurrentScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}