using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class PracticePooledParticleVFX : HaiMonoBehaviour, IPracticePoolable
{
    [Header("References")]
    [SerializeField] private ParticleSystem targetParticleSystem;

    [Header("Lifetime Settings")]
    [SerializeField, Min(0.05f)] private float defaultDuration = 0.5f;

    private float despawnTime;
    private bool isPlaying;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadParticleSystem();
    }

    protected override void Awake()
    {
        base.Awake();
        LoadParticleSystem();
    }

    private void LoadParticleSystem()
    {
        if (targetParticleSystem != null)
        {
            return;
        }

        targetParticleSystem = GetComponent<ParticleSystem>();
    }

    public void PlayBurst(
        Vector3 worldPosition,
        Color particleColor,
        int burstCount,
        float scaleMultiplier = 1f,
        float durationOverride = -1f
    )
    {
        LoadParticleSystem();
        transform.position = worldPosition;
        transform.localScale = Vector3.one * Mathf.Max(0.1f, scaleMultiplier);

        float duration = durationOverride > 0f ? durationOverride : defaultDuration;
        despawnTime = Time.time + duration;
        isPlaying = true;

        if (targetParticleSystem != null)
        {
            targetParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = targetParticleSystem.main;
            main.startColor = particleColor;

            targetParticleSystem.Emit(Mathf.Clamp(burstCount, 1, 60));
        }
    }

    private void Update()
    {
        if (!isPlaying)
        {
            return;
        }

        if (Time.time < despawnTime)
        {
            return;
        }

        isPlaying = false;
        PracticeObjectPoolManager.DespawnOrDestroy(gameObject);
    }

    public void OnTakenFromPool()
    {
        isPlaying = false;
    }

    public void OnReturnedToPool()
    {
        isPlaying = false;

        if (targetParticleSystem != null)
        {
            targetParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void ResetRuntimeState()
    {
        isPlaying = false;
        transform.localScale = Vector3.one;
    }
}

