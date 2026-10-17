using UnityEngine.SceneManagement;
using UnityEngine;

public class PracticeAudioManager : HaiMonoBehaviour
{
    protected static PracticeAudioManager instance;
    public static PracticeAudioManager Instance => instance;
    [Header("Audio Sources")]
    [SerializeField] protected AudioSource musicSource;
    [SerializeField] protected AudioSource sfxSource;
    [Header("Music Clips")]
    [SerializeField] protected AudioClip mainMenuMusic;
    [SerializeField] protected AudioClip gameplayMusic;
    [Header("Gameplay SFX")]
    [SerializeField] protected AudioClip shootSfx;
    [SerializeField] protected AudioClip enemyDeathSfx;
    [SerializeField] protected AudioClip playerHitSfx;
    [SerializeField] protected AudioClip levelUpSfx;
    [Header("UI SFX")]
    [SerializeField] protected AudioClip buttonClickSfx;
    protected override void Awake()
    {
        base.Awake();

        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;// vì khi instance null thì nó chọn hiện tại nên là this và kể từ đó destroy những object sau

        DontDestroyOnLoad(gameObject);
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
    protected void ConfigureAudioSources()
    {
        if(musicSource != null)
        {
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
        }
        if(sfxSource != null)
        {
            sfxSource.playOnAwake = false;
            sfxSource.loop = true;
            sfxSource.spatialBlend = 0f;
        }
    }
    protected void HandleSceneLoaded(Scene loadedScene, LoadSceneMode loadSceneMode)
    {
        PlayMusicForScene(loadedScene.name);
    }
    protected void PlayMusicForCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        PlayMusicForScene(currentScene.name);
    }
    protected void PlayMusicForScene(string sceneName)
    {
        if(sceneName == PracticeSceneNames.MainMenu)
        {
            PlayMusic(mainMenuMusic);
        }
        if(sceneName == PracticeSceneNames.GamePlay)
        {
            PlayMusic(gameplayMusic);
        }
        StopMusic();
    }
    protected void PlayMusic(AudioClip musicClip)
    {
        if(musicSource == null || musicClip == null) return;
        if(musicSource.clip == musicClip && musicSource.isPlaying) return;

        musicSource.Stop();
        musicSource.clip = musicClip;
        musicSource.Play();
    }
    public void StopMusic()
    {
        if(musicSource == null) return;
        musicSource.Stop();
        musicSource.clip = null;
    }
    public void PlaySfx(AudioClip sfxClip, float volumeScale = 1f)
    {
        if(sfxSource == null || sfxClip == null) return;

        float validVolumeScale = Mathf.Clamp01(volumeScale);
        sfxSource.PlayOneShot(sfxClip, validVolumeScale);
    }
    public void PlayShootSfx()
    {
        PlaySfx(shootSfx);
    }
    public void PlayEnemyDeathSfx()
    {
        PlaySfx(enemyDeathSfx);
    }public void PlayPlayerHitSfx()
    {
        PlaySfx(playerHitSfx);
    }public void PlayLevelUpSfx()
    {
        PlaySfx(levelUpSfx);
    }public void PlayButtonClickSfx()
    {
        PlaySfx(buttonClickSfx);
    }
}
