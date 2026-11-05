using UnityEngine;

public class PracticeVFXManager : HaiMonoBehaviour
{
    public static PracticeVFXManager Instance { get; private set; }

    [Header("Pooled VFX Template (Optional - Auto-Created if Null)")]
    [SerializeField] private PracticePooledParticleVFX burstVfxPrefab;
    [SerializeField] private Transform vfxContainer;

    [Header("VFX Performance Budget (Zero-Lag Stress Test Protection)")]
    [SerializeField, Min(0.01f)] private float minHitVfxInterval = 0.035f;
    [SerializeField, Min(0.01f)] private float minDeathVfxInterval = 0.025f;
    [SerializeField, Min(5)] private int maxActiveVfxInstances = 45;

    [Header("Palette Colors")]
    [SerializeField] private Color hitSparkColor = new Color(1f, 0.82f, 0.25f, 1f);
    [SerializeField] private Color enemyDeathColor = new Color(1f, 0.25f, 0.55f, 1f);
    [SerializeField] private Color playerHitColor = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color xpPickupColor = new Color(0.3f, 0.95f, 1f, 1f);
    [SerializeField] private Color levelUpColor = new Color(1f, 0.85f, 0.2f, 1f);

    private float lastHitVfxTime;
    private float lastDeathVfxTime;
    private static Sprite cachedParticleSprite;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        ResolveVfxContainer();
    }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();

        ResolveVfxContainer();
        EnsureRuntimeVfxPrefabAndPrewarm();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void ResolveVfxContainer()
    {
        if (vfxContainer != null)
        {
            return;
        }

        GameObject found = GameObject.Find("RuntimeVFX");
        if (found != null)
        {
            vfxContainer = found.transform;
            return;
        }

        GameObject gameplayRoot = GameObject.Find("PracticeGame/Gameplay");
        GameObject createdContainer = new GameObject("RuntimeVFX");
        if (gameplayRoot != null)
        {
            createdContainer.transform.SetParent(gameplayRoot.transform, false);
        }
        vfxContainer = createdContainer.transform;
    }

    private void EnsureRuntimeVfxPrefabAndPrewarm()
    {
        if (burstVfxPrefab == null)
        {
            burstVfxPrefab = CreateProceduralBurstPrefab();
        }

        if (PracticeObjectPoolManager.Instance != null && burstVfxPrefab != null)
        {
            PracticeObjectPoolManager.Instance.PrewarmPool(
                burstVfxPrefab.gameObject,
                24,
                vfxContainer
            );
        }
    }

    public void SpawnHitSpark(Vector3 position)
    {
        if (Time.unscaledTime - lastHitVfxTime < minHitVfxInterval)
        {
            return;
        }

        if (!HasVfxBudget())
        {
            return;
        }

        lastHitVfxTime = Time.unscaledTime;
        SpawnBurstInternal(position, hitSparkColor, 14, 1.15f, 0.32f);
    }

    public void SpawnEnemyDeathBurst(Vector3 position, Color? customColor = null)
    {
        if (Time.unscaledTime - lastDeathVfxTime < minDeathVfxInterval)
        {
            return;
        }

        if (!HasVfxBudget())
        {
            return;
        }

        lastDeathVfxTime = Time.unscaledTime;
        Color burstColor = customColor.HasValue ? customColor.Value : enemyDeathColor;
        SpawnBurstInternal(position, burstColor, 28, 1.5f, 0.52f);
    }

    public void SpawnPlayerHitBurst(Vector3 position)
    {
        SpawnBurstInternal(position, playerHitColor, 16, 1.15f, 0.45f);
    }

    public void SpawnXpCollectSparkle(Vector3 position)
    {
        if (!HasVfxBudget())
        {
            return;
        }

        SpawnBurstInternal(position, xpPickupColor, 5, 0.65f, 0.25f);
    }

    public void SpawnLevelUpBurst(Vector3 position)
    {
        SpawnBurstInternal(position, levelUpColor, 28, 1.45f, 0.65f);
    }

    public void SpawnBossDeathExplosion(Vector3 position)
    {
        SpawnBurstInternal(position, levelUpColor, 45, 2.0f, 0.85f);
    }

    private bool HasVfxBudget()
    {
        if (vfxContainer == null)
        {
            return true;
        }

        int activeCount = 0;
        int totalChildren = vfxContainer.childCount;
        for (int i = 0; i < totalChildren; i++)
        {
            Transform child = vfxContainer.GetChild(i);
            if (child != null && child.gameObject.activeSelf)
            {
                activeCount++;
                if (activeCount >= maxActiveVfxInstances)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void SpawnBurstInternal(
        Vector3 position,
        Color color,
        int particleCount,
        float scale,
        float duration
    )
    {
        if (burstVfxPrefab == null)
        {
            burstVfxPrefab = CreateProceduralBurstPrefab();
        }

        ResolveVfxContainer();

        PracticePooledParticleVFX instance =
            PracticeObjectPoolManager.SpawnOrInstantiate(
                burstVfxPrefab,
                position,
                Quaternion.identity,
                vfxContainer
            );

        if (instance != null)
        {
            instance.PlayBurst(position, color, particleCount, scale, duration);
        }
    }

    private PracticePooledParticleVFX CreateProceduralBurstPrefab()
    {
        GameObject templateObj = new GameObject("Practice_ProceduralBurstVFX_Template");
        templateObj.transform.SetParent(transform, false);
        templateObj.SetActive(false);

        ParticleSystem ps = templateObj.AddComponent<ParticleSystem>();
        ParticleSystemRenderer psRenderer = templateObj.GetComponent<ParticleSystemRenderer>();

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = 0.45f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.18f, 0.38f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(3.5f, 7.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.28f);
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 128;

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.enabled = false;

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.18f;

        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve shrinkCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, shrinkCurve);

        if (psRenderer != null)
        {
            Shader spriteShader = Shader.Find("Sprites/Default");
            if (spriteShader != null)
            {
                Material mat = new Material(spriteShader);
                mat.mainTexture = GetOrCreateSoftQuadTexture();
                psRenderer.sharedMaterial = mat;
            }
            psRenderer.sortingLayerName = "VFX";
            psRenderer.sortingOrder = 20;
        }

        PracticePooledParticleVFX pooledVfx = templateObj.AddComponent<PracticePooledParticleVFX>();
        return pooledVfx;
    }

    private static Texture2D GetOrCreateSoftQuadTexture()
    {
        if (cachedParticleSprite != null && cachedParticleSprite.texture != null)
        {
            return cachedParticleSprite.texture;
        }

        const int size = 16;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float maxRadius = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(1f - (dist / maxRadius));
                alpha = alpha * alpha;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        tex.Apply();
        cachedParticleSprite = Sprite.Create(
            tex,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            16f
        );
        return tex;
    }
}

