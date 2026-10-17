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

    protected override void OnEnable()
    {
        base.OnEnable();

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
        RefreshAllSliders();
        SetSettingsPanelActive(true);
    }

    private void HandleBackClicked()
    {
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