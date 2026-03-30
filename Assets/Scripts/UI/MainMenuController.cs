using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : HaiMonoBehaviour
{
    private GamePauseController gamePauseController;

    private GameObject mainMenuRootObject;
    private GameObject hudRootObject;
    private GameObject levelUpPopupRootObject;
    private Button startButton;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadGamePauseController();
        LoadMainMenuRootObject();
        LoadHUDRootObject();
        LoadLevelUpPopupRootObject();
        LoadStartButton();
    }

    protected override void Start()
    {
        base.Start();
        OpenMainMenu();
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

        MainMenuRoot marker = UIHierarchyLookup.FindInParentCanvas<MainMenuRoot>(this);
        if (marker == null) return;

        mainMenuRootObject = marker.gameObject;
    }

    private void LoadHUDRootObject()
    {
        if (hudRootObject != null) return;

        HUDRoot marker = UIHierarchyLookup.FindInParentCanvas<HUDRoot>(this);
        if (marker == null) return;

        hudRootObject = marker.gameObject;
    }

    private void LoadLevelUpPopupRootObject()
    {
        if (levelUpPopupRootObject != null) return;

        LevelUpPopupRoot marker = UIHierarchyLookup.FindInParentCanvas<LevelUpPopupRoot>(this);
        if (marker == null) return;

        levelUpPopupRootObject = marker.gameObject;
    }

    private void LoadStartButton()
    {
        if (startButton != null) return;

        StartGameButtonUI marker = UIHierarchyLookup.FindInParentCanvas<StartGameButtonUI>(this);
        if (marker == null) return;

        startButton = marker.GetComponent<Button>();
    }

    private void SubscribeButton()
    {
        if (startButton != null)
        {
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

    private void HandleStartClicked()
    {
        Debug.Log("Start Game clicked", gameObject);

        CloseMainMenu();
        ShowHUD();
        HideLevelUpPopup();
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

        HideLevelUpPopup();
        PauseGame();
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

    private void PauseGame()
    {
        gamePauseController?.PauseGame();
    }

    private void ResumeGame()
    {
        gamePauseController?.ResumeGame();
    }
}