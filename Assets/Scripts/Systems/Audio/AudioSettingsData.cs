using UnityEngine;

public static class AudioSettingsData
{
    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SfxVolume";

    public static float MusicVolume => PlayerPrefs.GetFloat(MusicVolumeKey, 0.8f);
    public static float SfxVolume => PlayerPrefs.GetFloat(SfxVolumeKey, 0.8f);

    public static void SaveMusicVolume(float value)
    {
        PlayerPrefs.SetFloat(MusicVolumeKey, Mathf.Clamp01(value));
        PlayerPrefs.Save();
    }

    public static void SaveSfxVolume(float value)
    {
        PlayerPrefs.SetFloat(SfxVolumeKey, Mathf.Clamp01(value));
        PlayerPrefs.Save();
    }
}