using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PracticeAudioManager : HaiMonoBehaviour
{
    protected static PracticeAudioManager instance;
    public static PracticeAudioManager Instance => instance;

    [Header("Audio Sources")]
    [SerializeField]
    protected AudioSource musicSource;

    [SerializeField]
    protected AudioSource sfxSource;

    [Header("Data-Driven Audio Library (Optional)")]
    [SerializeField]
    protected PracticeAudioLibrary audioLibrary;

    [Header("Music Clips (Direct Inspector Fallback)")]
    [SerializeField]
    protected AudioClip mainMenuMusic;

    [SerializeField]
    protected AudioClip gameplayMusic;

    [SerializeField]
    protected AudioClip bossMusic;

    [Header("Gameplay SFX (Direct Inspector Fallback)")]
    [SerializeField]
    protected AudioClip shootSfx;

    [SerializeField]
    protected AudioClip enemyDeathSfx;

    [SerializeField]
    protected AudioClip playerHitSfx;

    [SerializeField]
    protected AudioClip levelUpSfx;

    [Header("UI & Result SFX (Direct Inspector Fallback)")]
    [SerializeField]
    protected AudioClip buttonClickSfx;

    [SerializeField]
    protected AudioClip upgradePickSfx;

    [SerializeField]
    protected AudioClip gameOverSfx;

    [SerializeField]
    protected AudioClip victorySfx;

    [Header("Voice Budget & Persistence")]
    [SerializeField]
    protected bool persistAcrossScenes = true;

    [SerializeField, Min(0.01f)]
    protected float minSfxRepeatInterval = 0.045f;

    private readonly Dictionary<AudioClip, float> lastSfxPlayTimes =
        new Dictionary<AudioClip, float>();

    public PracticeAudioLibrary Library => audioLibrary;
    public AudioSource MusicSource => musicSource;
    public AudioSource SfxSource => sfxSource;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadAudioSources();
    }

    protected override void Awake()
    {
        if (instance != null && instance != this)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }

        instance = this;

        if (persistAcrossScenes)
        {
            Transform rootTransform = transform.root != null ? transform.root : transform;
            DontDestroyOnLoad(rootTransform.gameObject);
        }

        base.Awake();
        LoadAudioSources();
        ConfigureAudioSources();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    protected override void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        base.OnDisable();
    }

    protected override void Start()
    {
        base.Start();
        ConfigureAudioSources();
        PlayMusicForCurrentScene();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    protected void LoadAudioSources()
    {
        if (musicSource == null)
        {
            Transform musicChild = transform.Find("MusicSource");
            if (musicChild != null)
            {
                musicSource = musicChild.GetComponent<AudioSource>();
            }
        }

        if (sfxSource == null)
        {
            Transform sfxChild = transform.Find("SfxSource");
            if (sfxChild != null)
            {
                sfxSource = sfxChild.GetComponent<AudioSource>();
            }
        }
    }

    protected void ConfigureAudioSources()
    {
        if (musicSource != null)
        {
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
        }

        if (sfxSource != null)
        {
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f;
        }
    }

    protected void HandleSceneLoaded(Scene loadedScene, LoadSceneMode loadSceneMode)
    {
        PlayMusicForScene(loadedScene.name);
    }

    public void PlayMusicForCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        PlayMusicForScene(currentScene.name);
    }

    public void PlayMusicForScene(string sceneName)
    {
        if (sceneName == PracticeSceneNames.MainMenu)
        {
            PlayMenuMusic();
            return;
        }

        if (sceneName == PracticeSceneNames.GamePlay)
        {
            PlayGameplayMusic();
            return;
        }

        StopMusic();
    }

    public void PlayMusic(AudioClip musicClip, float pitch = 1f)
    {
        if (musicSource == null || musicClip == null)
        {
            return;
        }

        musicSource.pitch = Mathf.Clamp(pitch, 0.5f, 2f);

        if (musicSource.clip == musicClip && musicSource.isPlaying)
        {
            return;
        }

        musicSource.Stop();
        musicSource.clip = musicClip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource == null)
        {
            return;
        }

        musicSource.Stop();
        musicSource.clip = null;
        musicSource.pitch = 1f;
    }

    public void PlaySfx(AudioClip sfxClip, float volumeScale = 1f)
    {
        if (sfxSource == null || sfxClip == null)
        {
            return;
        }

        if (!CanPlaySfxWithinBudget(sfxClip))
        {
            return;
        }

        float validVolumeScale = Mathf.Clamp01(volumeScale);
        sfxSource.PlayOneShot(sfxClip, validVolumeScale);
    }

    // =========================================================
    // BGM HELPERS
    // =========================================================

    public void PlayMenuMusic()
    {
        AudioClip clip = mainMenuMusic != null
            ? mainMenuMusic
            : (audioLibrary != null ? audioLibrary.MenuBgm : null);

        PlayMusic(clip, 1f);
    }

    public void PlayGameplayMusic()
    {
        AudioClip clip = gameplayMusic != null
            ? gameplayMusic
            : (audioLibrary != null ? audioLibrary.GameplayBgm : null);

        PlayMusic(clip, 1f);
    }

    public void PlayBossMusic()
    {
        AudioClip clip = bossMusic != null
            ? bossMusic
            : (audioLibrary != null ? audioLibrary.BossBgm : gameplayMusic);

        AudioClip normalGameClip = gameplayMusic != null
            ? gameplayMusic
            : (audioLibrary != null ? audioLibrary.GameplayBgm : null);

        // Nếu dùng chung file nhạc với Gameplay thì tăng nhẹ pitch lên 1.06 để tạo nhịp dồn dập khi đánh Boss
        float pitch = (clip != null && clip == normalGameClip) ? 1.06f : 1f;
        PlayMusic(clip, pitch);
    }

    // =========================================================
    // SFX HELPERS (Tương thích 100% với toàn bộ Caller trong dự án)
    // =========================================================

    public void PlayShootSfx()
    {
        AudioClip clip = shootSfx != null
            ? shootSfx
            : (audioLibrary != null ? audioLibrary.ShootSfx : null);

        PlaySfx(clip);
    }

    public void PlayEnemyDeathSfx()
    {
        AudioClip clip = enemyDeathSfx != null
            ? enemyDeathSfx
            : (audioLibrary != null ? audioLibrary.EnemyDeathSfx : null);

        PlaySfx(clip);
    }

    public void PlayPlayerHitSfx()
    {
        AudioClip clip = playerHitSfx != null
            ? playerHitSfx
            : (audioLibrary != null ? audioLibrary.PlayerHitSfx : null);

        PlaySfx(clip);
    }

    public void PlayLevelUpSfx()
    {
        AudioClip clip = levelUpSfx != null
            ? levelUpSfx
            : (audioLibrary != null ? audioLibrary.LevelUpSfx : null);

        PlaySfx(clip);
    }

    public void PlayButtonClickSfx()
    {
        AudioClip clip = buttonClickSfx != null
            ? buttonClickSfx
            : (audioLibrary != null ? audioLibrary.ButtonClickSfx : null);

        PlaySfx(clip);
    }

    public void PlayUpgradePickSfx()
    {
        AudioClip clip = upgradePickSfx != null
            ? upgradePickSfx
            : (audioLibrary != null ? audioLibrary.UpgradePickSfx : levelUpSfx);

        PlaySfx(clip);
    }

    public void PlayGameOverSfx()
    {
        AudioClip clip = gameOverSfx != null
            ? gameOverSfx
            : (audioLibrary != null ? audioLibrary.GameOverSfx : null);

        PlaySfx(clip);
    }

    public void PlayVictorySfx()
    {
        AudioClip clip = victorySfx != null
            ? victorySfx
            : (audioLibrary != null ? audioLibrary.VictorySfx : levelUpSfx);

        PlaySfx(clip);
    }

    private bool CanPlaySfxWithinBudget(AudioClip clip)
    {
        float currentTime = Time.unscaledTime;

        if (lastSfxPlayTimes.TryGetValue(clip, out float lastTime))
        {
            if (currentTime - lastTime < minSfxRepeatInterval)
            {
                return false;
            }
        }

        lastSfxPlayTimes[clip] = currentTime;
        return true;
    }
}