using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PracticePlayerDamageAura : HaiMonoBehaviour
{
    [Header("Aura Combat Stats")]
    [SerializeField]
    private bool isAuraActive = true;

    [SerializeField, Min(1f)]
    private float auraDamage = 8f;

    [SerializeField, Min(0.5f)]
    private float auraRadius = 2.4f;

    [SerializeField, Min(0.1f)]
    private float damageTickInterval = 0.45f;

    [Header("Procedural Ring Visuals")]
    [SerializeField]
    private LineRenderer ringLineRenderer;

    [SerializeField, Range(24, 96)]
    private int circleSegments = 48;

    [SerializeField, Min(0.02f)]
    private float ringLineWidth = 0.08f;

    [SerializeField]
    private Color auraColor = new Color(0.18f, 0.85f, 1f, 0.65f);

    [SerializeField, Min(0.5f)]
    private float pulseSpeed = 4f;

    [SerializeField, Range(0f, 0.15f)]
    private float radiusPulseAmplitude = 0.05f;

    private float nextDamageTickTime;
    private float lastDrawnRadius = -1f;

    public bool IsAuraActive => isAuraActive;
    public float AuraDamage => auraDamage;
    public float AuraRadius => auraRadius;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadLineRenderer();
    }

    protected override void Awake()
    {
        base.Awake();
        LoadLineRenderer();
        ConfigureLineRenderer();
        UpdateCircleVertices(auraRadius);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        nextDamageTickTime = Time.time + damageTickInterval;
        UpdateVisualState();
    }

    private void LoadLineRenderer()
    {
        if (ringLineRenderer != null) return;

        ringLineRenderer = GetComponent<LineRenderer>();
    }

    private void ConfigureLineRenderer()
    {
        if (ringLineRenderer == null) return;

        ringLineRenderer.useWorldSpace = false;
        ringLineRenderer.loop = true;
        ringLineRenderer.positionCount = circleSegments;
        ringLineRenderer.startWidth = ringLineWidth;
        ringLineRenderer.endWidth = ringLineWidth;
        ringLineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        ringLineRenderer.receiveShadows = false;
        ringLineRenderer.sortingOrder = 1;

        // Sử dụng shader Sprites/Default có sẵn của Unity để hiển thị màu sắc trong suốt đẹp mắt
        if (ringLineRenderer.sharedMaterial == null)
        {
            Shader defaultSpriteShader = Shader.Find("Sprites/Default");
            if (defaultSpriteShader != null)
            {
                ringLineRenderer.material = new Material(defaultSpriteShader);
            }
        }

        ringLineRenderer.startColor = auraColor;
        ringLineRenderer.endColor = auraColor;
    }

    private void Update()
    {
        if (!isAuraActive)
        {
            return;
        }

        AnimateAuraRing();

        if (Time.time >= nextDamageTickTime)
        {
            nextDamageTickTime = Time.time + damageTickInterval;
            DealAuraDamageToTargetsInRange();
        }
    }

    private void AnimateAuraRing()
    {
        if (ringLineRenderer == null) return;

        float pulseOffset = Mathf.Sin(Time.time * pulseSpeed) * radiusPulseAmplitude;
        float animatedRadius = Mathf.Max(0.2f, auraRadius + pulseOffset);

        if (Mathf.Abs(animatedRadius - lastDrawnRadius) > 0.005f)
        {
            UpdateCircleVertices(animatedRadius);
        }
    }

    private void UpdateCircleVertices(float radius)
    {
        if (ringLineRenderer == null) return;

        lastDrawnRadius = radius;
        if (ringLineRenderer.positionCount != circleSegments)
        {
            ringLineRenderer.positionCount = circleSegments;
        }

        float angleStep = (Mathf.PI * 2f) / circleSegments;

        for (int i = 0; i < circleSegments; i++)
        {
            float angle = angleStep * i;
            Vector3 localPos = new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f
            );
            ringLineRenderer.SetPosition(i, localPos);
        }
    }

    private void DealAuraDamageToTargetsInRange()
    {
        if (PracticeTargetRegistry.Count == 0)
        {
            return;
        }

        IReadOnlyList<PracticeEnemyHealth> activeTargets = PracticeTargetRegistry.ActiveTargets;//dùng để giảm chi phí CPU gần như bằng 0
        Vector2 origin = transform.position;
        float radiusSqr = auraRadius * auraRadius;

        // Duyệt ngược từ cuối danh sách về đầu vì quái trúng sát thương Aura có thể chết ngay và tự rút tên khỏi Registry
        for (int i = activeTargets.Count - 1; i >= 0; i--)
        {
            if (i >= activeTargets.Count)
            {
                continue;
            }

            PracticeEnemyHealth target = activeTargets[i];

            if (target == null || target.IsDead || !target.gameObject.activeInHierarchy)
            {
                continue;
            }

            Vector2 offset = (Vector2)target.transform.position - origin;

            if (offset.sqrMagnitude <= radiusSqr)
            {
                target.TakeDamage(auraDamage);
            }
        }
    }

    public void AddAuraDamage(float amount)
    {
        if (amount <= 0f) return;

        isAuraActive = true;
        auraDamage += amount;
        UpdateVisualState();
    }

    public void AddAuraRadius(float amount)
    {
        if (amount <= 0f) return;

        isAuraActive = true;
        auraRadius += amount;
        UpdateCircleVertices(auraRadius);
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        if (ringLineRenderer != null)
        {
            ringLineRenderer.enabled = isAuraActive;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, auraRadius);
    }
}