using System;
using UnityEngine;
using UnityEngine.Audio;

public class PracticeAudioSettings : HaiMonoBehaviour
{
    private const string MasterVolumeKey =
        "PracticeMasterVolume";

    private const string MusicVolumeKey =
        "PracticeMusicVolume";

    private const string SfxVolumeKey =
        "PracticeSfxVolume";

    private const string MasterVolumeParameter =
        "MasterVolume";

    private const string MusicVolumeParameter =
        "MusicVolume";

    private const string SfxVolumeParameter =
        "SfxVolume";

    private const float MinimumLinearVolume = 0.0001f;

    [Header("Audio Mixer")]
    [SerializeField]
    private AudioMixer audioMixer;

    [Header("Default Volume")]
    [SerializeField, Range(0f, 1f)]
    private float defaultMasterVolume = 1f;

    [SerializeField, Range(0f, 1f)]
    private float defaultMusicVolume = 0.8f;

    [SerializeField, Range(0f, 1f)]
    private float defaultSfxVolume = 1f;

    private float masterVolume;
    private float musicVolume;
    private float sfxVolume;

    public event Action<float> MasterVolumeChanged;
    public event Action<float> MusicVolumeChanged;
    public event Action<float> SfxVolumeChanged;

    public float MasterVolume => masterVolume;
    public float MusicVolume => musicVolume;
    public float SfxVolume => sfxVolume;

    protected override void Start()
    {
        base.Start();

        LoadSettings();
        ApplyAllVolumes();
        NotifyAllVolumeChanged();
    }

    // =========================================================
    // PUBLIC SETTERS
    // =========================================================

    public void SetMasterVolume(float newVolume)
    {
        masterVolume =
            Mathf.Clamp01(newVolume);

        ApplyVolume(
            MasterVolumeParameter,
            masterVolume
        );

        PlayerPrefs.SetFloat(
            MasterVolumeKey,
            masterVolume
        );

        PlayerPrefs.Save();

        MasterVolumeChanged?.
            Invoke(masterVolume);
    }

    public void SetMusicVolume(float newVolume)
    {
        musicVolume =
            Mathf.Clamp01(newVolume);

        ApplyVolume(
            MusicVolumeParameter,
            musicVolume
        );

        PlayerPrefs.SetFloat(
            MusicVolumeKey,
            musicVolume
        );

        PlayerPrefs.Save();

        MusicVolumeChanged?.
            Invoke(musicVolume);
    }

    public void SetSfxVolume(float newVolume)
    {
        sfxVolume =
            Mathf.Clamp01(newVolume);

        ApplyVolume(
            SfxVolumeParameter,
            sfxVolume
        );

        PlayerPrefs.SetFloat(
            SfxVolumeKey,
            sfxVolume
        );

        PlayerPrefs.Save();

        SfxVolumeChanged?.
            Invoke(sfxVolume);
    }

    // =========================================================
    // LOAD / APPLY
    // =========================================================

    private void LoadSettings()
    {
        masterVolume =
            PlayerPrefs.GetFloat(
                MasterVolumeKey,
                defaultMasterVolume
            );

        musicVolume =
            PlayerPrefs.GetFloat(
                MusicVolumeKey,
                defaultMusicVolume
            );

        sfxVolume =
            PlayerPrefs.GetFloat(
                SfxVolumeKey,
                defaultSfxVolume
            );

        masterVolume =
            Mathf.Clamp01(masterVolume);

        musicVolume =
            Mathf.Clamp01(musicVolume);

        sfxVolume =
            Mathf.Clamp01(sfxVolume);
    }

    private void ApplyAllVolumes()
    {
        ApplyVolume(
            MasterVolumeParameter,
            masterVolume
        );

        ApplyVolume(
            MusicVolumeParameter,
            musicVolume
        );

        ApplyVolume(
            SfxVolumeParameter,
            sfxVolume
        );
    }

    private void ApplyVolume(
        string parameterName,
        float linearVolume
    )
    {
        if (audioMixer == null)
        {
            Debug.LogError(
                $"{name}: Audio Mixer đang null.",
                this
            );

            return;
        }

        float decibelVolume =
            ConvertLinearToDecibel(
                linearVolume
            );

        bool appliedSuccessfully =
            audioMixer.SetFloat(
                parameterName,
                decibelVolume
            );

        if (!appliedSuccessfully)
        {
            Debug.LogWarning(
                $"{name}: Không tìm thấy exposed parameter " +
                $"{parameterName}.",
                this
            );
        }
    }

    private float ConvertLinearToDecibel(
        float linearVolume
    )
    {
        if (linearVolume <= 0f)
        {
            return -80f;// -80 là gần như là im lặng
        }

        float safeLinearVolume =
            Mathf.Max(
                linearVolume,
                MinimumLinearVolume
            );

        return Mathf.Log10(
            safeLinearVolume
        ) * 20f;//đây là công thức tính decibel
    }

    private void NotifyAllVolumeChanged()
    {
        MasterVolumeChanged?.
            Invoke(masterVolume);

        MusicVolumeChanged?.
            Invoke(musicVolume);

        SfxVolumeChanged?.
            Invoke(sfxVolume);
    }

    // =========================================================
    // DEBUG RESET
    // =========================================================

    [ContextMenu("Reset Audio Settings")]
    private void ResetAudioSettings()
    {
        PlayerPrefs.DeleteKey(
            MasterVolumeKey
        );

        PlayerPrefs.DeleteKey(
            MusicVolumeKey
        );

        PlayerPrefs.DeleteKey(
            SfxVolumeKey
        );

        PlayerPrefs.Save();

        masterVolume =
            defaultMasterVolume;

        musicVolume =
            defaultMusicVolume;

        sfxVolume =
            defaultSfxVolume;

        ApplyAllVolumes();
        NotifyAllVolumeChanged();
    }
}