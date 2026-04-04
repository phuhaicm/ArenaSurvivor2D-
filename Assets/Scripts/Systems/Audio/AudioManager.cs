using UnityEngine;

public class AudioManager : HaiMonoBehaviour
{
    private AudioSource musicSource;
    private AudioSource sfxSource;

    private AudioCueKey? currentMusicKey;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadMusicSource();
        LoadSfxSource();
    }

    private void LoadMusicSource()
    {
        if (musicSource != null) return;

        MusicAudioSourceRoot marker = GetComponentInChildren<MusicAudioSourceRoot>(true);
        if (marker == null) return;

        musicSource = marker.GetComponent<AudioSource>();
    }

    private void LoadSfxSource()
    {
        if (sfxSource != null) return;

        SfxAudioSourceRoot marker = GetComponentInChildren<SfxAudioSourceRoot>(true);
        if (marker == null) return;

        sfxSource = marker.GetComponent<AudioSource>();
    }

    public void PlayMusic(AudioCueKey key, bool loop = true)
    {
        if (musicSource == null) return;
        if (currentMusicKey == key) return;

        AudioClip clip = LoadClip(key);
        if (clip == null) return;

        currentMusicKey = key;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void PlaySfx(AudioCueKey key)
    {
        if (sfxSource == null) return;

        AudioClip clip = LoadClip(key);
        if (clip == null) return;

        sfxSource.PlayOneShot(clip);
    }

    private AudioClip LoadClip(AudioCueKey key)
    {
        string path = AudioCueCatalog.GetResourcePath(key);
        AudioClip clip = Resources.Load<AudioClip>(path);

        if (clip == null)
        {
            Debug.LogWarning($"Audio clip not found at Resources path: {path}");
        }

        return clip;
    }
}