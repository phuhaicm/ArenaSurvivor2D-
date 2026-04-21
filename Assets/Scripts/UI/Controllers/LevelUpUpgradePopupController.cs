using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpUpgradePopupController : HaiMonoBehaviour
{
    private PlayerLevelSystem playerLevelSystem;
    private PlayerUpgradeApplier playerUpgradeApplier;
    private GamePauseController gamePauseController;
    private AudioManager audioManager;
    private SurvivalTimer survivalTimer;
    private LevelUpPopupRoot popupRoot;

    private GameObject popupRootObject;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI bodyText;
    private UpgradeChoiceButtonUI[] upgradeButtons;
    private Button rerollButton;

    private int pendingLevelUpCount;
    private int remainingRerolls;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadPlayerLevelSystem();
        LoadPlayerUpgradeApplier();
        LoadGamePauseController();
        LoadAudioManager();
        LoadSurvivalTimer();
        LoadPopupRoot();
        LoadPopupRootObject();
        LoadTitleText();
        LoadBodyText();
        LoadUpgradeButtons();
        LoadRerollButton();
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
        SubscribeButtons();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        UnsubscribeEvents();
        UnsubscribeButtons();
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

    private void LoadAudioManager()
    {
        if (audioManager != null) return;
        audioManager = FindFirstObjectByType<AudioManager>();
    }

    private void LoadSurvivalTimer()
    {
        if (survivalTimer != null) return;
        survivalTimer = FindFirstObjectByType<SurvivalTimer>();
    }

    private void LoadPopupRoot()
    {
        if (popupRoot != null) return;
        popupRoot = UIRootLookup.FindRootInCanvas<LevelUpPopupRoot>(this);
    }

    private void LoadPopupRootObject()
    {
        if (popupRootObject != null) return;
        if (popupRoot == null) return;

        popupRootObject = popupRoot.gameObject;
    }

    private void LoadTitleText()
    {
        if (titleText != null) return;
        if (popupRoot == null) return;

        LevelUpPopupTitleUI marker = popupRoot.GetComponentInChildren<LevelUpPopupTitleUI>(true);
        if (marker == null) return;

        titleText = marker.GetComponent<TextMeshProUGUI>();
    }

    private void LoadBodyText()
    {
        if (bodyText != null) return;
        if (popupRoot == null) return;

        LevelUpPopupBodyTextUI marker = popupRoot.GetComponentInChildren<LevelUpPopupBodyTextUI>(true);
        if (marker == null) return;

        bodyText = marker.GetComponent<TextMeshProUGUI>();
    }

    private void LoadUpgradeButtons()
    {
        if (upgradeButtons != null && upgradeButtons.Length > 0) return;
        if (popupRoot == null) return;

        UpgradeButtonsRoot root = popupRoot.GetComponentInChildren<UpgradeButtonsRoot>(true);
        if (root == null) return;

        upgradeButtons = root.GetComponentsInChildren<UpgradeChoiceButtonUI>(true);
    }

    private void LoadRerollButton()
    {
        if (rerollButton != null) return;
        if (popupRoot == null) return;

        LevelUpPopupRerollButtonUI marker = popupRoot.GetComponentInChildren<LevelUpPopupRerollButtonUI>(true);
        if (marker == null) return;

        rerollButton = marker.GetComponent<Button>();
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

    private void SubscribeButtons()
    {
        if (rerollButton != null)
        {
            rerollButton.onClick.RemoveListener(HandleRerollClicked);
            rerollButton.onClick.AddListener(HandleRerollClicked);
        }
    }

    private void UnsubscribeButtons()
    {
        if (rerollButton != null)
        {
            rerollButton.onClick.RemoveListener(HandleRerollClicked);
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

    private void HandleRerollClicked()
    {
        if (remainingRerolls <= 0)
        {
            return;
        }

        remainingRerolls--;
        audioManager?.PlaySfx(AudioCueKey.ButtonClick);

        ClearAllButtons();
        BindUpgradeButtons();
        RefreshTexts();
        RefreshRerollButtonState();
    }

    private void OpenUpgradeSelection()
    {
        remainingRerolls = 1;

        ShowPopup();
        PauseGame();
        RefreshTexts();
        BindUpgradeButtons();
        RefreshRerollButtonState();
    }

    private void BindUpgradeButtons()
    {
        if (upgradeButtons == null || upgradeButtons.Length == 0) return;

        float survivalTime = survivalTimer != null ? survivalTimer.ElapsedTime : 0f;
        List<UpgradeOptionData> options = UpgradeOptionRoller.RollUniqueOptions(upgradeButtons.Length, survivalTime);

        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            if (i >= options.Count) continue;

            UpgradeOptionData option = options[i];
            upgradeButtons[i].Bind(option, () => HandleUpgradeSelected(option));
        }
    }

    private void HandleUpgradeSelected(UpgradeOptionData option)
    {
        audioManager?.PlaySfx(AudioCueKey.UpgradePick);
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
            titleText.text = "LEVEL UP!";
        }

        if (bodyText != null && playerLevelSystem != null)
        {
            bodyText.text =
                $"You reached Level {playerLevelSystem.CurrentLevel}\n" +
                $"Choose 1 upgrade\n" +
                $"Free Rerolls Left: {remainingRerolls}";
        }
    }

    private void RefreshRerollButtonState()
    {
        if (rerollButton == null) return;

        rerollButton.interactable = remainingRerolls > 0;

        TextMeshProUGUI label = rerollButton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null)
        {
            label.text = remainingRerolls > 0 ? "REROLL" : "NO REROLLS";
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