using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : HaiMonoBehaviour
{
    private GamePauseController gamePauseController;

    private GameObject mainMenuRootObject;
    private GameObject hudRootObject;
    private GameObject levelUpPopupRootObject;
    private GameObject gameOverRootObject;
    private Button startButton;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadGamePauseController();
        LoadMainMenuRootObject();
        LoadHUDRootObject();
        LoadLevelUpPopupRootObject();
        LoadGameOverRootObject();
        LoadStartButton();
    }

    protected override void Start()
    {
        base.Start();
        HandleInitialBootState();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SubscribeButton();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        UnsubscribeButton();
    }

    private void LoadGamePauseController()
    {
        if (gamePauseController != null) return;
        gamePauseController = FindFirstObjectByType<GamePauseController>();
    }

    private void LoadMainMenuRootObject()
    {
        if (mainMenuRootObject != null) return;

        MainMenuRoot marker = UIRootLookup.FindRootInCanvas<MainMenuRoot>(this);
        if (marker == null) return;

        mainMenuRootObject = marker.gameObject;
    }

    private void LoadHUDRootObject()
    {
        if (hudRootObject != null) return;

        HUDRoot marker = UIRootLookup.FindRootInCanvas<HUDRoot>(this);
        if (marker == null) return;

        hudRootObject = marker.gameObject;
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

    private void LoadStartButton()
    {
        if (startButton != null) return;

        StartGameButtonUI marker = UIRootLookup.FindInRoot<MainMenuRoot, StartGameButtonUI>(this);
        if (marker == null) return;

        startButton = marker.GetComponent<Button>();
    }

    private void SubscribeButton()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(HandleStartClicked);
            startButton.onClick.AddListener(HandleStartClicked);
        }
    }

    private void UnsubscribeButton()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(HandleStartClicked);
        }
    }

    private void HandleInitialBootState()
    {
        HideLevelUpPopup();
        HideGameOver();

        if (GameBootFlow.StartIntoGameplay)
        {
            GameBootFlow.StartIntoGameplay = false;
            StartGameplayImmediately();
            return;
        }

        OpenMainMenu();
    }

    private void HandleStartClicked()
    {
        CloseMainMenu();
        ShowHUD();
        HideLevelUpPopup();
        HideGameOver();
        ResumeGame();
    }

    private void OpenMainMenu()
    {
        if (mainMenuRootObject != null)
        {
            mainMenuRootObject.SetActive(true);
        }

        if (hudRootObject != null)
        {
            hudRootObject.SetActive(false);
        }

        PauseGame();
    }

    private void StartGameplayImmediately()
    {
        if (mainMenuRootObject != null)
        {
            mainMenuRootObject.SetActive(false);
        }

        ShowHUD();
        ResumeGame();
    }

    private void CloseMainMenu()
    {
        if (mainMenuRootObject != null)
        {
            mainMenuRootObject.SetActive(false);
        }
    }

    private void ShowHUD()
    {
        if (hudRootObject != null)
        {
            hudRootObject.SetActive(true);
        }
    }

    private void HideLevelUpPopup()
    {
        if (levelUpPopupRootObject != null)
        {
            levelUpPopupRootObject.SetActive(false);
        }
    }

    private void HideGameOver()
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

    private void ResumeGame()
    {
        gamePauseController?.ResumeGame();
    }
}