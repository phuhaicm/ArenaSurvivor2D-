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

    [Header("Procedural Electric Plasma Edge")]
    [SerializeField]
    private LineRenderer ringLineRenderer;

    [SerializeField, Range(24, 96)]
    private int circleSegments = 54;

    [SerializeField, Min(0.02f)]
    private float ringLineWidth = 0.12f;

    [SerializeField]
    private Color auraColor = new Color(0.25f, 0.95f, 1f, 0.9f);

    [SerializeField, Min(0.5f)]
    private float pulseSpeed = 4f;

    [SerializeField, Range(0f, 0.15f)]
    private float radiusPulseAmplitude = 0.05f;

    [Header("Electric Wave Noise")]
    [SerializeField]
    private bool enablePlasmaWave = true;

    [SerializeField]
    private float waveFrequency = 6f;

    [SerializeField]
    private float waveSpeed = 8f;

    [SerializeField]
    private float waveAmplitude = 0.05f;

    [Header("Dual Rotating Plasma Shield Rings")]
    [SerializeField]
    private SpriteRenderer outerRingRenderer;

    [SerializeField]
    private SpriteRenderer innerRingRenderer;

    [SerializeField]
    private Sprite plasmaRingSprite;

    [SerializeField]
    private float outerRotationSpeed = 60f;

    [SerializeField]
    private float innerRotationSpeed = 42f;

    [SerializeField]
    private float spriteBaseUnitDiameter = 0.853f; // 512px / 600 PPU

    private float nextDamageTickTime;
    private float lastDrawnRadius = -1f;

    public bool IsAuraActive => isAuraActive;
    public float AuraDamage => auraDamage;
    public float AuraRadius => auraRadius;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadLineRenderer();
        EnsurePlasmaVisuals();
    }

    protected override void Awake()
    {
        base.Awake();
        LoadLineRenderer();
        EnsurePlasmaVisuals();
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

    private void EnsurePlasmaVisuals()
    {
        if (outerRingRenderer == null)
        {
            Transform outerChild = transform.Find("AuraShieldOuter");
            if (outerChild != null)
            {
                outerRingRenderer = outerChild.GetComponent<SpriteRenderer>();
            }
        }

        if (innerRingRenderer == null)
        {
            Transform innerChild = transform.Find("AuraShieldInner");
            if (innerChild != null)
            {
                innerRingRenderer = innerChild.GetComponent<SpriteRenderer>();
            }
        }
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
        ringLineRenderer.sortingLayerName = "VFX";
        ringLineRenderer.sortingOrder = 10;

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
        float pulseOffset = Mathf.Sin(Time.time * pulseSpeed) * radiusPulseAmplitude;
        float animatedRadius = Mathf.Max(0.2f, auraRadius + pulseOffset);

        // Luôn cập nhật viền điện plasma mỗi frame để sóng điện rung rinh
        if (ringLineRenderer != null && ringLineRenderer.enabled)
        {
            UpdateCircleVertices(animatedRadius);
        }

        float baseDiameter = spriteBaseUnitDiameter > 0.05f ? spriteBaseUnitDiameter : 1f;

        // Vòng ngoài quay theo chiều kim đồng hồ
        if (outerRingRenderer != null && outerRingRenderer.enabled)
        {
            outerRingRenderer.transform.Rotate(0f, 0f, -outerRotationSpeed * Time.deltaTime);
            float outerDiameter = (animatedRadius * 2f) / baseDiameter;
            outerRingRenderer.transform.localScale = new Vector3(outerDiameter, outerDiameter, 1f);
        }

        // Vòng trong quay ngược chiều và nhấp nháy nhịp thở năng lượng
        if (innerRingRenderer != null && innerRingRenderer.enabled)
        {
            innerRingRenderer.transform.Rotate(0f, 0f, innerRotationSpeed * Time.deltaTime);
            float innerDiameter = (animatedRadius * 1.55f) / baseDiameter;
            innerRingRenderer.transform.localScale = new Vector3(innerDiameter, innerDiameter, 1f);

            Color c = innerRingRenderer.color;
            c.a = 0.35f + Mathf.Sin(Time.time * (pulseSpeed * 1.5f)) * 0.15f;
            innerRingRenderer.color = c;
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
        float time = Time.time;

        for (int i = 0; i < circleSegments; i++)
        {
            float angle = angleStep * i;
            float r = radius;

            if (enablePlasmaWave)
            {
                // Sóng điện tích plasma đa hài biến thiên liên tục, tạo viền điện giật sống động
                float ripple = Mathf.Sin(angle * waveFrequency + time * waveSpeed) * waveAmplitude
                             + Mathf.Cos(angle * (waveFrequency * 2f) - time * (waveSpeed * 1.4f)) * (waveAmplitude * 0.5f);
                r += ripple;
            }

            Vector3 localPos = new Vector3(
                Mathf.Cos(angle) * r,
                Mathf.Sin(angle) * r,
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

        if (outerRingRenderer != null)
        {
            outerRingRenderer.enabled = isAuraActive;
        }

        if (innerRingRenderer != null)
        {
            innerRingRenderer.enabled = isAuraActive;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, auraRadius);
    }
}
