using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpPopupController : HaiMonoBehaviour
{
    private PlayerLevelSystem playerLevelSystem;
    private GamePauseController gamePauseController;

    private GameObject popupRootObject;
    private TextMeshProUGUI titleText;
    private Button continueButton;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadPlayerLevelSystem();
        LoadGamePauseController();
        LoadPopupRootObject();
        LoadTitleText();
        LoadContinueButton();
    }

    protected override void Start()
    {
        base.Start();
        HidePopupInstant();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SubscribeEvents();
        SubscribeButton();
        RefreshPopupText();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        UnsubscribeEvents();
        UnsubscribeButton();
    }

    private void LoadPlayerLevelSystem()
    {
        if (playerLevelSystem != null) return;
        playerLevelSystem = FindFirstObjectByType<PlayerLevelSystem>();
    }

    private void LoadGamePauseController()
    {
        if (gamePauseController != null) return;
        gamePauseController = FindFirstObjectByType<GamePauseController>();
    }

    private void LoadPopupRootObject()
    {
        if (popupRootObject != null) return;

        LevelUpPopupRoot marker = UIHierarchyLookup.FindInParentCanvas<LevelUpPopupRoot>(this);
        if (marker == null) return;

        popupRootObject = marker.gameObject;
    }

    private void LoadTitleText()
    {
        if (titleText != null) return;

        LevelUpPopupTitleUI marker = UIHierarchyLookup.FindInParentCanvas<LevelUpPopupTitleUI>(this);
        if (marker == null) return;

        titleText = marker.GetComponent<TextMeshProUGUI>();
    }

    private void LoadContinueButton()
    {
        if (continueButton != null) return;

        LevelUpPopupContinueButtonUI marker = UIHierarchyLookup.FindInParentCanvas<LevelUpPopupContinueButtonUI>(this);
        if (marker == null) return;

        continueButton = marker.GetComponent<Button>();
    }

    private void SubscribeEvents()
    {
        if (playerLevelSystem != null)
        {
            playerLevelSystem.LeveledUp += HandleLeveledUp;
        }
    }

    private void UnsubscribeEvents()
    {
        if (playerLevelSystem != null)
        {
            playerLevelSystem.LeveledUp -= HandleLeveledUp;
        }
    }

    private void SubscribeButton()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(HandleContinueClicked);
        }
    }

    private void UnsubscribeButton()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(HandleContinueClicked);
        }
    }

    private void HandleLeveledUp(int newLevel)
    {
        ShowPopup(newLevel);
        PauseGame();
    }

    private void HandleContinueClicked()
    {
        HidePopup();
        ResumeGame();
    }

    private void ShowPopup(int level)
    {
        if (popupRootObject != null)
        {
            popupRootObject.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = $"Level Up!\nLevel {level}";
        }
    }

    private void HidePopup()
    {
        if (popupRootObject != null)
        {
            popupRootObject.SetActive(false);
        }
    }

    private void HidePopupInstant()
    {
        HidePopup();
    }

    private void RefreshPopupText()
    {
        if (titleText != null && playerLevelSystem != null)
        {
            titleText.text = $"Level Up!\nLevel {playerLevelSystem.CurrentLevel}";
        }
    }

    private void PauseGame()
    {
        if (gamePauseController != null)
        {
            gamePauseController.PauseGame();
        }
    }

    private void ResumeGame()
    {
        if (gamePauseController != null)
        {
            gamePauseController.ResumeGame();
        }
    }
}