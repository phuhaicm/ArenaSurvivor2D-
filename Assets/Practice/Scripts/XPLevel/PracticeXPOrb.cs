using UnityEngine;

public class PracticeXPOrb : HaiMonoBehaviour, IPracticePoolable
{
    [SerializeField] private int experienceAmount = 1;

    [Header("Procedural Pulse Juice")]
    [SerializeField] private bool enablePulseAnimation = true;
    [SerializeField, Range(0f, 0.35f)] private float pulseAmplitude = 0.12f;
    [SerializeField, Min(0.5f)] private float pulseSpeed = 4.5f;

    [Header("Procedural Spin Juice")]
    [SerializeField] private bool enableRotation = true;
    [SerializeField] private float rotationSpeed = 75f;

    private Vector3 initialScale = Vector3.one;
    private bool hasCachedInitialScale;
    private float pulsePhaseOffset;

    public int ExperienceAmount => experienceAmount;

    protected override void Awake()
    {
        base.Awake();
        CacheInitialScale();
        pulsePhaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        CacheInitialScale();
        transform.localScale = initialScale;
    }

    protected override void ResetValues()
    {
        base.ResetValues();
        experienceAmount = 1;
        enablePulseAnimation = true;
        pulseAmplitude = 0.12f;
        pulseSpeed = 4.5f;
        enableRotation = true;
        rotationSpeed = 75f;
    }

    private void CacheInitialScale()
    {
        if (hasCachedInitialScale)
        {
            return;
        }

        initialScale = transform.localScale;
        if (initialScale.sqrMagnitude < 0.001f)
        {
            initialScale = Vector3.one;
        }

        hasCachedInitialScale = true;
    }

    private void Update()
    {
        if (enableRotation)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }

        if (!enablePulseAnimation)
        {
            return;
        }

        float scaleFactor = 1f + Mathf.Sin(Time.time * pulseSpeed + pulsePhaseOffset) * pulseAmplitude;
        transform.localScale = initialScale * scaleFactor;
    }

    public void OnTakenFromPool()
    {
        CacheInitialScale();
        transform.localScale = initialScale;
    }

    public void OnReturnedToPool()
    {
        CacheInitialScale();
        transform.localScale = initialScale;
    }

    public void ResetRuntimeState()
    {
        CacheInitialScale();
        transform.localScale = initialScale;
    }
}
