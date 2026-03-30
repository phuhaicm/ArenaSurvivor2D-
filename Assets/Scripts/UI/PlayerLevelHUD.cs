using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLevelHUD : HaiMonoBehaviour
{
    private PlayerLevelSystem playerLevelSystem;
    private HUDRoot hudRoot;

    private TextMeshProUGUI levelText;
    private TextMeshProUGUI experienceText;
    private Image experienceFillImage;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadPlayerLevelSystem();
        LoadHUDRoot();
        LoadLevelText();
        LoadExperienceText();
        LoadExperienceFillImage();
    }

    protected override void Start()
    {
        base.Start();
        RefreshUI();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SubscribeEvents();
        RefreshUI();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        UnsubscribeEvents();
    }

    private void LoadPlayerLevelSystem()
    {
        if (playerLevelSystem != null) return;

        playerLevelSystem = FindFirstObjectByType<PlayerLevelSystem>();

        if (playerLevelSystem != null)
        {
            LogLoad(nameof(LoadPlayerLevelSystem));
        }
    }

    private void LoadHUDRoot()
    {
        if (hudRoot != null) return;

        hudRoot = UIRootLookup.FindRootInCanvas<HUDRoot>(this);

        if (hudRoot != null)
        {
            LogLoad(nameof(LoadHUDRoot));
        }
    }

    private void LoadLevelText()
    {
        if (levelText != null) return;

        LevelTextUI marker = UIRootLookup.FindInRoot<HUDRoot, LevelTextUI>(this);
        if (marker == null) return;

        levelText = marker.GetComponent<TextMeshProUGUI>();
    }

    private void LoadExperienceText()
    {
        if (experienceText != null) return;

        ExperienceTextUI marker = UIRootLookup.FindInRoot<HUDRoot, ExperienceTextUI>(this);
        if (marker == null) return;

        experienceText = marker.GetComponent<TextMeshProUGUI>();
    }

    private void LoadExperienceFillImage()
    {
        if (experienceFillImage != null) return;

        ExperienceFillUI marker = UIRootLookup.FindInRoot<HUDRoot, ExperienceFillUI>(this);
        if (marker == null) return;

        experienceFillImage = marker.GetComponent<Image>();
    }

    private void SubscribeEvents()
    {
        if (playerLevelSystem != null)
        {
            playerLevelSystem.LevelStateChanged += RefreshUI;
        }
    }

    private void UnsubscribeEvents()
    {
        if (playerLevelSystem != null)
        {
            playerLevelSystem.LevelStateChanged -= RefreshUI;
        }
    }

    private void RefreshUI()
    {
        if (playerLevelSystem == null) return;

        if (levelText != null)
        {
            levelText.text = $"Level {playerLevelSystem.CurrentLevel}";
        }

        if (experienceText != null)
        {
            experienceText.text = $"{playerLevelSystem.CurrentLevelExperience} / {playerLevelSystem.RequiredExperience}";
        }

        if (experienceFillImage != null)
        {
            experienceFillImage.fillAmount = playerLevelSystem.ProgressNormalized;
        }
    }
}
