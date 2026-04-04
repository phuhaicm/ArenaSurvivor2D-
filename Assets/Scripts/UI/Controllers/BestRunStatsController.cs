using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsController : HaiMonoBehaviour
{
    private AudioManager audioManager;
    private AudioSettingsPanelRoot settingsRoot;

    private GameObject settingsRootObject;
    private Slider musicSlider;
    private Slider sfxSlider;
    private Button openButton;
    private Button closeButton;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadAudioManager();
        LoadSettingsRoot();
        LoadSettingsRootObject();
        LoadMusicSlider();
        LoadSfxSlider();
        LoadOpenButton();
        LoadCloseButton();
    }

    protected override void Start()
    {
        base.Start();
        HideSettingsInstant();
        SyncSlidersFromAudio();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SubscribeButtons();
        SubscribeSliders();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        UnsubscribeButtons();
        UnsubscribeSliders();
    }

    private void LoadAudioManager()
    {
        if (audioManager != null) return;
        audioManager = FindFirstObjectByType<AudioManager>();
    }

    private void LoadSettingsRoot()
    {
        if (settingsRoot != null) return;
        settingsRoot = UIRootLookup.FindRootInCanvas<AudioSettingsPanelRoot>(this);
    }

    private void LoadSettingsRootObject()
    {
        if (settingsRootObject != null) return;
        if (settingsRoot == null) return;

        settingsRootObject = settingsRoot.gameObject;
    }

    private void LoadMusicSlider()
    {
        if (musicSlider != null) return;
        if (settingsRoot == null) return;

        MusicVolumeSliderUI marker = settingsRoot.GetComponentInChildren<MusicVolumeSliderUI>(true);
        if (marker == null) return;

        musicSlider = marker.GetComponent<Slider>();
    }

    private void LoadSfxSlider()
    {
        if (sfxSlider != null) return;
        if (settingsRoot == null) return;

        SfxVolumeSliderUI marker = settingsRoot.GetComponentInChildren<SfxVolumeSliderUI>(true);
        if (marker == null) return;

        sfxSlider = marker.GetComponent<Slider>();
    }

    private void LoadOpenButton()
    {
        if (openButton != null) return;

        AudioSettingsOpenButtonUI marker = UIRootLookup.FindInRoot<MainMenuRoot, AudioSettingsOpenButtonUI>(this);
        if (marker == null) return;

        openButton = marker.GetComponent<Button>();
    }

    private void LoadCloseButton()
    {
        if (closeButton != null) return;
        if (settingsRoot == null) return;

        AudioSettingsCloseButtonUI marker = settingsRoot.GetComponentInChildren<AudioSettingsCloseButtonUI>(true);
        if (marker == null) return;

        closeButton = marker.GetComponent<Button>();
    }

    private void SubscribeButtons()
    {
        if (openButton != null)
        {
            openButton.onClick.RemoveListener(HandleOpenClicked);
            openButton.onClick.AddListener(HandleOpenClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(HandleCloseClicked);
            closeButton.onClick.AddListener(HandleCloseClicked);
        }
    }

    private void UnsubscribeButtons()
    {
        if (openButton != null)
        {
            openButton.onClick.RemoveListener(HandleOpenClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(HandleCloseClicked);
        }
    }

    private void SubscribeSliders()
    {
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(HandleMusicVolumeChanged);
            musicSlider.onValueChanged.AddListener(HandleMusicVolumeChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(HandleSfxVolumeChanged);
            sfxSlider.onValueChanged.AddListener(HandleSfxVolumeChanged);
        }
    }

    private void UnsubscribeSliders()
    {
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(HandleMusicVolumeChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(HandleSfxVolumeChanged);
        }
    }

    private void HandleOpenClicked()
    {
        ShowSettings();
        SyncSlidersFromAudio();
    }

    private void HandleCloseClicked()
    {
        HideSettings();
    }

    private void HandleMusicVolumeChanged(float value)
    {
        audioManager?.SetMusicVolume(value);
    }

    private void HandleSfxVolumeChanged(float value)
    {
        audioManager?.SetSfxVolume(value);
    }

    private void SyncSlidersFromAudio()
    {
        if (audioManager == null) return;

        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(audioManager.MusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(audioManager.SfxVolume);
        }
    }

    private void ShowSettings()
    {
        if (settingsRootObject != null)
        {
            settingsRootObject.SetActive(true);
        }
    }

    private void HideSettings()
    {
        if (settingsRootObject != null)
        {
            settingsRootObject.SetActive(false);
        }
    }

    private void HideSettingsInstant()
    {
        HideSettings();
    }
}