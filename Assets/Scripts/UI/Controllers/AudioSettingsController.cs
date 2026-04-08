using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsController : HaiMonoBehaviour
{
    private AudioManager audioManager;
    private AudioSettingsPanelRoot settingsRoot;

    private GameObject settingsRootObject;
    private GameObject pauseMenuRootObject;
    private GameObject mainMenuRootObject;

    private Slider musicSlider;
    private Slider sfxSlider;
    private Button[] openButtons;
    private Button closeButton;

    private bool openedFromPauseMenu;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadAudioManager();
        LoadSettingsRoot();
        LoadSettingsRootObject();
        LoadPauseMenuRootObject();
        LoadMainMenuRootObject();
        LoadMusicSlider();
        LoadSfxSlider();
        LoadOpenButtons();
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

    private void LoadPauseMenuRootObject()
    {
        if (pauseMenuRootObject != null) return;

        PauseMenuRoot marker = UIRootLookup.FindRootInCanvas<PauseMenuRoot>(this);
        if (marker == null) return;

        pauseMenuRootObject = marker.gameObject;
    }

    private void LoadMainMenuRootObject()
    {
        if (mainMenuRootObject != null) return;

        MainMenuRoot marker = UIRootLookup.FindRootInCanvas<MainMenuRoot>(this);
        if (marker == null) return;

        mainMenuRootObject = marker.gameObject;
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

    private void LoadOpenButtons()
    {
        if (openButtons != null && openButtons.Length > 0) return;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        AudioSettingsOpenButtonUI[] markers = canvas.GetComponentsInChildren<AudioSettingsOpenButtonUI>(true);
        if (markers == null || markers.Length == 0) return;

        openButtons = new Button[markers.Length];

        for (int i = 0; i < markers.Length; i++)
        {
            openButtons[i] = markers[i].GetComponent<Button>();
        }
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
        if (openButtons != null)
        {
            for (int i = 0; i < openButtons.Length; i++)
            {
                if (openButtons[i] == null) continue;

                openButtons[i].onClick.RemoveListener(HandleOpenClicked);
                openButtons[i].onClick.AddListener(HandleOpenClicked);
            }
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(HandleCloseClicked);
            closeButton.onClick.AddListener(HandleCloseClicked);
        }
    }

    private void UnsubscribeButtons()
    {
        if (openButtons != null)
        {
            for (int i = 0; i < openButtons.Length; i++)
            {
                if (openButtons[i] == null) continue;
                openButtons[i].onClick.RemoveListener(HandleOpenClicked);
            }
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
        openedFromPauseMenu = pauseMenuRootObject != null && pauseMenuRootObject.activeSelf;

        if (openedFromPauseMenu && pauseMenuRootObject != null)
        {
            pauseMenuRootObject.SetActive(false);
        }

        ShowSettings();
        SyncSlidersFromAudio();
    }

    private void HandleCloseClicked()
    {
        HideSettings();

        if (openedFromPauseMenu && pauseMenuRootObject != null)
        {
            pauseMenuRootObject.SetActive(true);
        }

        openedFromPauseMenu = false;
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
            settingsRootObject.transform.SetAsLastSibling();
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
