//using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class PracticeUpgradeChoiceUI : HaiMonoBehaviour
{
    [Header("References")]
    [SerializeField] private PracticePlayerLevelSystem levelSystem;
    [SerializeField] private PracticeShooterUpgradeApplier upgradeApplier;
    [SerializeField] private PracticeGameStateController gameStateController;

    [Header("Upgrade Choices")]
    [SerializeField] private PracticeUpgradeDefinition[] upgradePool;
    [Header("UI References")]
    [SerializeField] private GameObject upgradePanel;
    [Header("Choice Âctions")]
    [SerializeField] private Button rerollButton;
    [SerializeField] private Button skipButton;
    [Header("Reroll Settings")]
    [SerializeField, Min(0)] private int rerollPerChoices = 1;//lượt reroll
    [SerializeField] private int remainingRerolls;//số reroll hiện có

    [SerializeField] private TextMeshProUGUI titleText;
    //[SerializeField] private Button damageButton;
    //[SerializeField] private Button fireRateButton;
    //[SerializeField] private Button projectileButton;
    [SerializeField] private Button[] choiceButtons;

    private int pendingUpgradeChoices;
    private readonly List<PracticeUpgradeDefinition>
    currentChoices = new();//choices hiện trên màn hình UI 3 choices
    private readonly List<PracticeUpgradeDefinition>
    availableChoice = new();// đây là mảng tạm cho upgrade
    private readonly Dictionary<PracticeUpgradeDefinition, int> upgradeLevels = new();//lưu upgrade level run time
    protected override void LoadComponents()
    {
        base.LoadComponents();

        LoadLevelSystem();
        LoadUpgradeApplier();
        LoadGameStateController();
    }

    protected virtual void LoadLevelSystem()
    {
        if (levelSystem != null) return;

        levelSystem = FindFirstObjectByType<PracticePlayerLevelSystem>();
    }

    protected virtual void LoadUpgradeApplier()
    {
        if (upgradeApplier != null) return;

        upgradeApplier = FindFirstObjectByType<PracticeShooterUpgradeApplier>();
    }
    protected virtual void LoadGameStateController()
    {
        if (gameStateController != null) return;

        gameStateController = FindFirstObjectByType<PracticeGameStateController>();
    }

    protected override void Start()
    {
        base.Start();

        //HidePanel();
        if(upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
        RefeshButtonTexts();
        RefeshRerollButton();
        RefeshSkipButton();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        SubscribeLevelEvents();
        SubscribeButtonEvents();
    }

    protected override void OnDisable()
    {
        
        UnsubscribeLevelEvents();
        UnsubscribeButtonEvents();
        base.OnDisable();
    }
    protected void BuildAvailableChoices()//lúc nào cũng phải tạo 1 mảng tạm để làm việc ko đụng cái gốc trong inspector
    {
        availableChoice.Clear();
        
        foreach(PracticeUpgradeDefinition upgrade in upgradePool)
        {
            if(upgrade == null) continue;
            if(HasReachedMaxiumLevel(upgrade)) continue;
            availableChoice.Add(upgrade);
        }
    }
    protected int GetUpgradeLevel(PracticeUpgradeDefinition upgrade)
    {
        if(upgrade == null) return 0;
        if(upgradeLevels.TryGetValue(upgrade, out int currentLevel))// trả 2 kết quả dictionary có chưa upgrade hay ko nếu có thì level hiện tại là bao nhiêu
        {
            return currentLevel;
        }
        return 0;
    }
    protected bool HasReachedMaxiumLevel(PracticeUpgradeDefinition upgrade)
    {
        if(upgrade == null) return true;
        int currentLevel = GetUpgradeLevel(upgrade);
        
        return currentLevel >= upgrade.MaxLevel;
    }
    protected void IncreaseUpgradeLevel(PracticeUpgradeDefinition upgrade)//nâng level run time
    {
        if(upgrade == null) return;
        int currentLevel = GetUpgradeLevel(upgrade);
        int nextLevel = currentLevel + 1;

        upgradeLevels[upgrade] = nextLevel;
    }
    protected bool GenerateRandomChoices()
    {
        currentChoices.Clear();
        BuildAvailableChoices();

        int choiceCount = Mathf.Min(3, availableChoice.Count);
        for(int i = 0; i < choiceCount; i++)//ko có = tại vì nếu choiceCount là 2 thì chỉ cần random 1 lần 
        {
            int randomIndex = Random.Range(0, availableChoice.Count);
            PracticeUpgradeDefinition selectedUpgrade = availableChoice[randomIndex];

            currentChoices.Add(selectedUpgrade);
            availableChoice.RemoveAt(randomIndex);//phải xóa để tránh trùng random kiểu như random chọn số đó r thì sau đó phải xóa
        }
        return currentChoices.Count > 0;
    }
    private void SubscribeLevelEvents()
    {
        if (levelSystem == null) return;

        levelSystem.LevelChanged += HandleLeveledUp;
    }

    private void UnsubscribeLevelEvents()
    {
        if (levelSystem == null) return;

        levelSystem.LevelChanged -= HandleLeveledUp;
    }

    private void SubscribeButtonEvents()
    {
        if(choiceButtons == null || choiceButtons.Length < 3) return;

        if(choiceButtons[0] != null)
        {
            choiceButtons[0].onClick.AddListener(HandleButtonChoice1Clicked);
        }
        if(choiceButtons[1] != null)
        {
            choiceButtons[1].onClick.AddListener(HandleButtonChoice2Clicked);
        }
        if(choiceButtons[2] != null)
        {
            choiceButtons[2].onClick.AddListener(HandleButtonChoice3Clicked);
        }
        if(rerollButton != null)
        {
            rerollButton.onClick.AddListener(HandleRerollButtonClicked);
        }
        if(skipButton != null)
        {
            skipButton.onClick.AddListener(HandleSkipButtonClicked);
        }
    }
    private void UnsubscribeButtonEvents()
    {
        if(choiceButtons == null || choiceButtons.Length < 3) return;

        if(choiceButtons[0] != null)
        {
            choiceButtons[0].onClick.RemoveListener(HandleButtonChoice1Clicked);
        }
        if(choiceButtons[1] != null)
        {
            choiceButtons[1].onClick.RemoveListener(HandleButtonChoice2Clicked);
        }
        if(choiceButtons[2] != null)
        {
            choiceButtons[2].onClick.RemoveListener(HandleButtonChoice3Clicked);
        }
        if(rerollButton != null)
        {
            rerollButton.onClick.RemoveListener(HandleRerollButtonClicked);
        }
        if(skipButton != null)
        {
            skipButton.onClick.RemoveListener(HandleSkipButtonClicked);
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
        bool shoulUpgradePanelMenu = gameStates == PracticeGameStates.Paused;

        SetUpgradePanelMenu(shoulUpgradePanelMenu);
        if(gameStates != PracticeGameStates.GameOver) return;

        pendingUpgradeChoices = 0;
        currentChoices.Clear();
        RefeshButtonTexts();
        RefeshRerollButton();
        RefeshSkipButton();
    }
    protected void SetUpgradePanelMenu(bool isPause)
    {
        if(upgradePanel == null) return;

        upgradePanel.SetActive(isPause);
    }
    protected void HandleRerollButtonClicked()
    {
        if(pendingUpgradeChoices <= 0) return;
        if(remainingRerolls <= 0) return;
        remainingRerolls--;

        bool hasReroll = GenerateRandomChoices();
        if (!hasReroll)
        {
            pendingUpgradeChoices = 0;
            currentChoices.Clear();
            HidePanel();
            return;
        }
        RefeshButtonTexts();
        RefeshRerollButton();
    }
    protected void HandleSkipButtonClicked()
    {
        if(pendingUpgradeChoices <= 0) return;
        Debug.Log($"{name}: upgrade choice skipped.",this);
        CompleteOneChoice();
    }
    protected void RefeshRerollButton()
    {
        if(rerollButton == null) return;
        bool canReroll = pendingUpgradeChoices > 0 && remainingRerolls > 0 && currentChoices.Count > 0;
        rerollButton.interactable = canReroll;

        SetButtonText(rerollButton, $"Reroll ({remainingRerolls})");
    }

    private void HandleLeveledUp(float newLevel)
    {
        PracticeAudioManager.Instance?.PlayLevelUpSfx();
        pendingUpgradeChoices++;

        if(titleText != null)
        {
            titleText.text = $"Level {newLevel} reached! Choose an upgrade:";
        }
        if(pendingUpgradeChoices > 1) return;//nghĩa là đã có 1 lượt đang hiển thị chỉ ghi nhận thêm pending chứ ko thay đổi currentchoices
        
        bool prepareSuccesfull = PrepareNextChoices();
        if(prepareSuccesfull) return; // có nghĩa là nếu thành công rồi thì xong tương đương == true

        pendingUpgradeChoices = 0;
        HidePanel();

        Debug.Log($"{name}: Không còn upgrade hợp lệ",this);        
    }
    protected bool PrepareNextChoices()
    {
        remainingRerolls = rerollPerChoices;
        bool hasReroll = GenerateRandomChoices();

        if(!hasReroll) return false;

        RefeshButtonTexts();
        RefeshRerollButton();
        RefeshSkipButton();
        ShowPanel();
        return true;
    }

    private void HandleButtonChoice1Clicked()
    {
        ApplyChoice(0);
    }
    private void HandleButtonChoice2Clicked()
    {
        ApplyChoice(1);
    }
    private void HandleButtonChoice3Clicked()
    {
        ApplyChoice(2);
    } 
    private void ApplyChoice(int choiceIndex)
    {
        if (!CanChooseUpgrade())
        {
            Debug.LogWarning($"{name}: Cannot choose upgrade. ", this);
        }
        //if(upgradePool == null) return;
        if(choiceIndex < 0 || choiceIndex >= currentChoices.Count)
        {
            Debug.LogWarning($"{name}: Upgrade choice index" + $"{choiceIndex}: không hợp lệ", this);
            return;
        }

        PracticeUpgradeDefinition selectedUpgrade = currentChoices[choiceIndex];//currentChoices là đang hiện 3 upgrade ngẫu nhiên rồi nếu để upgradepool thì sẽ bị kiểu ko còn random nữa mà chọn từ trên xuống
        int currentUpgradeLevel = GetUpgradeLevel(selectedUpgrade);
        int nextUpgradeLevel = currentUpgradeLevel + 1;
        bool appliedSuccesfully = upgradeApplier.ApplyUpgrade(selectedUpgrade, nextUpgradeLevel);

        if(!appliedSuccesfully) return;
        IncreaseUpgradeLevel(selectedUpgrade);
        CompleteOneChoice();
    }
    private bool CanChooseUpgrade()
    {
        if(pendingUpgradeChoices <= 0) return false;
        if(upgradeApplier == null ) return false;

        return true;
    }
    private void CompleteOneChoice()
    {
        pendingUpgradeChoices = Mathf.Max(0, pendingUpgradeChoices - 1);

        if(pendingUpgradeChoices > 0)
        {
            bool hasRerollChoices = PrepareNextChoices();

            if(hasRerollChoices) return;
            pendingUpgradeChoices = 0;
        }
        currentChoices.Clear();
        HidePanel();
    }
    protected void RefeshSkipButton()
    {
        if(skipButton == null) return;
        skipButton.interactable = pendingUpgradeChoices > 0;
    }
    private void ShowPanel()
    {
        if(gameStateController == null) return;

        bool stateChanged = gameStateController.RequestUpgradeChoices();
        if(!stateChanged && !gameStateController.IsChoosingUpgrade) return;

        if(upgradePanel != null)
        {
            upgradePanel.SetActive(true);
        }
    }
    private void HidePanel(){
        if(upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
        if(rerollButton != null)
        {
            rerollButton.interactable = false;//thuộc gameObject với lại dùng interactable thì vẫn hiện thị button như ko bấm còn sài gameObject.SetActive(false) là vô hiệu hóa và mất nút luôn
        }
        if(skipButton != null)
        {
            skipButton.interactable = false;
        }
        if(gameStateController == null) return;

        if (gameStateController.IsChoosingUpgrade)
        {
            gameStateController.RequestRunning();
        }
    }
    private void RefeshButtonTexts()
    {
        if(choiceButtons == null) return;
        for(int i = 0; i < choiceButtons.Length; i++)
        {
            Button button = choiceButtons[i];

            if(button == null) continue;
            if(i >= currentChoices.Count)
            {
            button.interactable = false;
            SetButtonText(button, "No upgrade");

            continue;
            }

        PracticeUpgradeDefinition selectedUpgrade = currentChoices[i];

        if(selectedUpgrade == null)
        {
            button.interactable = false;//button bị vô hiệu hóa player ko thể click onclick ko chạy 
            SetButtonText(button, "Empty upgrade");

            continue;
        }
        
        button.interactable = true; 
        int currentUpgradeLevel = GetUpgradeLevel(selectedUpgrade);
        int nextUpgradeLevel = currentUpgradeLevel + 1;
        float nextValue = selectedUpgrade.GetValuePerLevel(nextUpgradeLevel);

        string content = $"{selectedUpgrade.DisplayName}\n" + $"{selectedUpgrade.Description}" + $"{selectedUpgrade.MaxLevel}" + $"value: {nextValue}" + $"Level: {nextUpgradeLevel}";
        SetButtonText(button, content);
        }
    }
    private void SetButtonText(Button button, string content)
    {
        if(button == null) return;

        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();

        if(buttonText == null) return;
        
        buttonText.text = content;
        
    }
}
