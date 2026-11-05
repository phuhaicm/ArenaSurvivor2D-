using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PracticeSettingsUI : HaiMonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PracticeAudioSettings audioSettings;

    [Header("Panels")]
    [SerializeField]
    private GameObject settingsPanel;

    [Header("Volume Sliders")]
    [SerializeField]
    private Slider masterVolumeSlider;

    [SerializeField]
    private Slider musicVolumeSlider;

    [SerializeField]
    private Slider sfxVolumeSlider;

    [Header("Buttons")]
    [SerializeField]
    private Button openSettingsButton;

    [SerializeField]
    private Button backButton;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadAudioSettings();
    }

    private void LoadAudioSettings()
    {
        if (audioSettings != null) return;
        audioSettings = FindFirstObjectByType<PracticeAudioSettings>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        LoadAudioSettings();
        SanitizeAllButtons();
        SubscribeSliderEvents();
        SubscribeButtonEvents();
    }

    protected override void OnDisable()
    {
        UnsubscribeSliderEvents();
        UnsubscribeButtonEvents();

        base.OnDisable();
    }

    protected override void Start()
    {
        base.Start();

        ConfigureSliders();
        RefreshAllSliders();
        SetSettingsPanelActive(false);
    }

    private void SanitizeAllButtons()
    {
        SanitizeButtonChildren(openSettingsButton);
        SanitizeButtonChildren(backButton);
    }

    private void SanitizeButtonChildren(Button parentButton)
    {
        if (parentButton == null) return;

        Button[] childButtons = parentButton.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < childButtons.Length; i++)
        {
            if (childButtons[i] != null && childButtons[i] != parentButton)
            {
                childButtons[i].enabled = false;
            }
        }

        TMP_Text[] childTexts = parentButton.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < childTexts.Length; i++)
        {
            if (childTexts[i] != null)
            {
                childTexts[i].raycastTarget = false;
            }
        }
    }

    // =========================================================
    // SETUP
    // =========================================================

    private void ConfigureSliders()
    {
        ConfigureSlider(masterVolumeSlider);
        ConfigureSlider(musicVolumeSlider);
        ConfigureSlider(sfxVolumeSlider);
    }

    private void ConfigureSlider(Slider slider)
    {
        if (slider == null)
        {
            return;
        }

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
    }

    private void RefreshAllSliders()
    {
        if (audioSettings == null)
        {
            LoadAudioSettings();
        }

        if (audioSettings == null)
        {
            return;
        }

        SetSliderValueWithoutNotify(
            masterVolumeSlider,
            audioSettings.MasterVolume
        );

        SetSliderValueWithoutNotify(
            musicVolumeSlider,
            audioSettings.MusicVolume
        );

        SetSliderValueWithoutNotify(
            sfxVolumeSlider,
            audioSettings.SfxVolume
        );
    }

    private void SetSliderValueWithoutNotify(
        Slider slider,
        float value
    )
    {
        if (slider == null)
        {
            return;
        }

        slider.SetValueWithoutNotify(
            Mathf.Clamp01(value)
        );
    }

    // =========================================================
    // SLIDER EVENTS
    // =========================================================

    private void SubscribeSliderEvents()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.
                RemoveListener(
                    HandleMasterVolumeChanged
                );

            masterVolumeSlider.onValueChanged.
                AddListener(
                    HandleMasterVolumeChanged
                );
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.
                RemoveListener(
                    HandleMusicVolumeChanged
                );

            musicVolumeSlider.onValueChanged.
                AddListener(
                    HandleMusicVolumeChanged
                );
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.
                RemoveListener(
                    HandleSfxVolumeChanged
                );

            sfxVolumeSlider.onValueChanged.
                AddListener(
                    HandleSfxVolumeChanged
                );
        }
    }

    private void UnsubscribeSliderEvents()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.
                RemoveListener(
                    HandleMasterVolumeChanged
                );
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.
                RemoveListener(
                    HandleMusicVolumeChanged
                );
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.
                RemoveListener(
                    HandleSfxVolumeChanged
                );
        }
    }

    private void HandleMasterVolumeChanged(
        float newVolume
    )
    {
        audioSettings?.
            SetMasterVolume(newVolume);
    }

    private void HandleMusicVolumeChanged(
        float newVolume
    )
    {
        audioSettings?.
            SetMusicVolume(newVolume);
    }

    private void HandleSfxVolumeChanged(
        float newVolume
    )
    {
        audioSettings?.
            SetSfxVolume(newVolume);
    }

    // =========================================================
    // BUTTON EVENTS
    // =========================================================

    private void SubscribeButtonEvents()
    {
        if (openSettingsButton != null)
        {
            openSettingsButton.onClick.
                RemoveListener(
                    HandleOpenSettingsClicked
                );

            openSettingsButton.onClick.
                AddListener(
                    HandleOpenSettingsClicked
                );
        }

        if (backButton != null)
        {
            backButton.onClick.
                RemoveListener(
                    HandleBackClicked
                );

            backButton.onClick.
                AddListener(
                    HandleBackClicked
                );
        }
    }

    private void UnsubscribeButtonEvents()
    {
        if (openSettingsButton != null)
        {
            openSettingsButton.onClick.
                RemoveListener(
                    HandleOpenSettingsClicked
                );
        }

        if (backButton != null)
        {
            backButton.onClick.
                RemoveListener(
                    HandleBackClicked
                );
        }
    }

    private void HandleOpenSettingsClicked()
    {
        PracticeAudioManager.Instance?.PlayButtonClickSfx();
        RefreshAllSliders();
        SetSettingsPanelActive(true);
    }

    private void HandleBackClicked()
    {
        PracticeAudioManager.Instance?.PlayButtonClickSfx();
        SetSettingsPanelActive(false);
    }

    // =========================================================
    // PANEL
    // =========================================================

    private void SetSettingsPanelActive(
        bool isActive
    )
    {
        if (settingsPanel == null)
        {
            return;
        }

        settingsPanel.SetActive(isActive);
    }
}
