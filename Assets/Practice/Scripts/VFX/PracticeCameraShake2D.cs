using UnityEngine;

public class PracticeCameraShake2D : HaiMonoBehaviour
{
    public static PracticeCameraShake2D Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PracticeFinalBossController finalBossController;
    [SerializeField] private PracticeGameStateController gameStateController;
    [SerializeField] private PracticePlayerLevelSystem playerLevelSystem;

    [Header("Shake Settings")]
    [SerializeField, Min(0f)] private float maxShakeOffset = 0.45f;
    [SerializeField, Min(1f)] private float shakeFrequency = 28f;

    private Vector3 initialLocalPosition;
    private bool hasCachedInitialPosition;
    private float remainingShakeTime;
    private float currentShakeDuration;
    private float currentShakeMagnitude;
    private float noiseSeed;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadSceneSystems();
    }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        base.Awake();

        CacheInitialPosition();
        noiseSeed = Random.Range(0f, 100f);
        LoadSceneSystems();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        CacheInitialPosition();
        LoadSceneSystems();
        SubscribeEvents();
    }

    protected override void OnDisable()
    {
        UnsubscribeEvents();
        ResetCameraPosition();
        base.OnDisable();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void CacheInitialPosition()
    {
        if (hasCachedInitialPosition)
        {
            return;
        }

        initialLocalPosition = transform.localPosition;
        hasCachedInitialPosition = true;
    }

    private void LoadSceneSystems()
    {
        if (finalBossController == null)
        {
            finalBossController = FindFirstObjectByType<PracticeFinalBossController>();
        }

        if (gameStateController == null)
        {
            gameStateController = FindFirstObjectByType<PracticeGameStateController>();
        }

        if (playerLevelSystem == null)
        {
            playerLevelSystem = FindFirstObjectByType<PracticePlayerLevelSystem>();
        }
    }

    private void SubscribeEvents()
    {
        if (finalBossController != null)
        {
            finalBossController.BossSpawned -= HandleBossSpawned;
            finalBossController.BossSpawned += HandleBossSpawned;

            finalBossController.BossDefeated -= HandleBossDefeated;
            finalBossController.BossDefeated += HandleBossDefeated;
        }

        if (gameStateController != null)
        {
            gameStateController.StateChanged -= HandleGameStateChanged;
            gameStateController.StateChanged += HandleGameStateChanged;
        }

        if (playerLevelSystem != null)
        {
            playerLevelSystem.LevelChanged -= HandlePlayerLevelUp;
            playerLevelSystem.LevelChanged += HandlePlayerLevelUp;
        }
    }

    private void UnsubscribeEvents()
    {
        if (finalBossController != null)
        {
            finalBossController.BossSpawned -= HandleBossSpawned;
            finalBossController.BossDefeated -= HandleBossDefeated;
        }

        if (gameStateController != null)
        {
            gameStateController.StateChanged -= HandleGameStateChanged;
        }

        if (playerLevelSystem != null)
        {
            playerLevelSystem.LevelChanged -= HandlePlayerLevelUp;
        }
    }

    private void LateUpdate()
    {
        if (remainingShakeTime <= 0f)
        {
            return;
        }

        if (gameStateController != null && !gameStateController.IsRunning)
        {
            ResetCameraPosition();
            return;
        }

        remainingShakeTime -= Time.deltaTime;

        if (remainingShakeTime <= 0f)
        {
            ResetCameraPosition();
            return;
        }

        float normalizedRemaining = currentShakeDuration > 0.001f
            ? Mathf.Clamp01(remainingShakeTime / currentShakeDuration)
            : 0f;

        // Giảm chấn bậc hai (Quadratic Damping) giúp chấn động mạnh lúc đầu và êm dần về cuối
        float dampedMagnitude = currentShakeMagnitude * (normalizedRemaining * normalizedRemaining);
        float timeCursor = Time.time * shakeFrequency;

        float offsetX = (Mathf.PerlinNoise(noiseSeed, timeCursor) * 2f - 1f) * dampedMagnitude;
        float offsetY = (Mathf.PerlinNoise(noiseSeed + 31.7f, timeCursor) * 2f - 1f) * dampedMagnitude;

        transform.localPosition = initialLocalPosition + new Vector3(offsetX, offsetY, 0f);
    }

    public void Shake(float intensity, float duration)
    {
        if (intensity <= 0f || duration <= 0f)
        {
            return;
        }

        CacheInitialPosition();

        float clampedIntensity = Mathf.Min(maxShakeOffset, intensity);

        // Nếu đang có một chấn động mạnh hơn chưa kết thúc thì ưu tiên giữ chấn động mạnh hơn
        if (remainingShakeTime > 0f && clampedIntensity < currentShakeMagnitude)
        {
            return;
        }

        currentShakeMagnitude = clampedIntensity;
        currentShakeDuration = duration;
        remainingShakeTime = duration;
    }

    public void ResetCameraPosition()
    {
        remainingShakeTime = 0f;
        currentShakeDuration = 0f;
        currentShakeMagnitude = 0f;

        if (hasCachedInitialPosition)
        {
            transform.localPosition = initialLocalPosition;
        }
    }

    private void HandleBossSpawned()
    {
        Shake(0.32f, 0.45f);
    }

    private void HandleBossDefeated()
    {
        Shake(0.42f, 0.55f);
        PracticeVFXManager.Instance?.SpawnBossDeathExplosion(Vector3.zero);
    }

    private void HandlePlayerLevelUp(float newLevel)
    {
        if (playerLevelSystem != null)
        {
            PracticeVFXManager.Instance?.SpawnLevelUpBurst(playerLevelSystem.transform.position);
        }
    }

    private void HandleGameStateChanged(PracticeGameStates newState)
    {
        if (newState != PracticeGameStates.Running)
        {
            ResetCameraPosition();
        }
    }
}

