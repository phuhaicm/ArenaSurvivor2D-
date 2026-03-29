using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelUpUpgradePopupController : HaiMonoBehaviour
{
    private PlayerLevelSystem playerLevelSystem;
    private PlayerUpgradeApplier playerUpgradeApplier;
    private GamePauseController gamePauseController;

    private GameObject popupRootObject;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI bodyText;
    private UpgradeChoiceButtonUI[] upgradeButtons;

    private int pendingLevelUpCount;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadPlayerLevelSystem();
        LoadPlayerUpgradeApplier();
        LoadGamePauseController();
        LoadPopupRootObject();
        LoadTitleText();
        LoadBodyText();
        LoadUpgradeButtons();
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
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        UnsubscribeEvents();
        ClearAllButtons();
    }

    private void LoadPlayerLevelSystem()
    {
        if (playerLevelSystem != null) return;
        playerLevelSystem = FindFirstObjectByType<PlayerLevelSystem>();
    }

    private void LoadPlayerUpgradeApplier()
    {
        if (playerUpgradeApplier != null) return;
        playerUpgradeApplier = FindFirstObjectByType<PlayerUpgradeApplier>();
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

    private void LoadBodyText()
    {
        if (bodyText != null) return;
        LevelUpPopupBodyTextUI marker = UIHierarchyLookup.FindInParentCanvas<LevelUpPopupBodyTextUI>(this);
        if (marker == null) return;
        bodyText = marker.GetComponent<TextMeshProUGUI>();
    }

    private void LoadUpgradeButtons()
    {
        if (upgradeButtons != null && upgradeButtons.Length > 0) return;

        UpgradeButtonsRoot root = UIHierarchyLookup.FindInParentCanvas<UpgradeButtonsRoot>(this);
        if (root == null) return;

        upgradeButtons = root.GetComponentsInChildren<UpgradeChoiceButtonUI>(true);
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

    private void HandleLeveledUp(int newLevel)
    {
        pendingLevelUpCount++;

        if (pendingLevelUpCount == 1)
        {
            OpenUpgradeSelection();
        }
    }

    private void OpenUpgradeSelection()
    {
        ShowPopup();
        PauseGame();
        RefreshTexts();
        BindUpgradeButtons();
    }

    private void BindUpgradeButtons()
    {
        if (upgradeButtons == null || upgradeButtons.Length == 0) return;

        List<UpgradeOptionData> options = PlayerUpgradeCatalog.GetRandomOptions(upgradeButtons.Length);

        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            if (i >= options.Count) continue;

            UpgradeOptionData option = options[i];
            upgradeButtons[i].Bind(option, () => HandleUpgradeSelected(option));
        }
    }

    private void HandleUpgradeSelected(UpgradeOptionData option)
    {
        playerUpgradeApplier?.ApplyUpgrade(option);

        pendingLevelUpCount = Mathf.Max(0, pendingLevelUpCount - 1);
        ClearAllButtons();

        if (pendingLevelUpCount > 0)
        {
            OpenUpgradeSelection();
            return;
        }

        HidePopup();
        ResumeGame();
    }

    private void RefreshTexts()
    {
        if (titleText != null)
        {
            titleText.text = $"LEVEL {playerLevelSystem.CurrentLevel}";
        }

        if (bodyText != null)
        {
            bodyText.text = "Choose 1 upgrade";
        }
    }

    private void ClearAllButtons()
    {
        if (upgradeButtons == null) return;

        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            upgradeButtons[i].ClearListeners();
        }
    }

    private void ShowPopup()
    {
        if (popupRootObject != null)
        {
            popupRootObject.SetActive(true);
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

    private void PauseGame()
    {
        gamePauseController?.PauseGame();
    }

    private void ResumeGame()
    {
        gamePauseController?.ResumeGame();
    }
}