using UnityEngine;

public class PracticeXPDropOnDeath : HaiMonoBehaviour
{
    // Lưu đệm Container ở cấp static để tránh gọi GameObject.Find("RuntimeXPOrbs") hàng trăm lần khi sinh quái
    private static Transform cachedXpOrbContainer;

    [Header("References")]
    [SerializeField]
    protected PracticeEnemyHealth enemyHealth;

    [SerializeField]
    protected GameObject xpOrbPrefab;

    [SerializeField]
    protected Transform xpOrbContainer;

    [Header("Burst Drop Settings (Elite / Boss)")]
    [Tooltip("Số lượng hạt XP văng ra khi chết (Quái thường = 1, Quái Elite = 8-12).")]
    [SerializeField, Min(1)]
    protected int dropCount = 1;

    [Tooltip("Bán kính văng tỏa tròn của chùm hạt XP khi dropCount > 1.")]
    [SerializeField, Min(0f)]
    protected float burstScatterRadius = 1.15f;

    protected override void LoadComponents()
    {
        base.LoadComponents();

        LoadEnemyHealth();
        LoadXPOrbContainer();
    }

    protected virtual void LoadEnemyHealth()
    {
        if (enemyHealth != null) return;

        enemyHealth = GetComponent<PracticeEnemyHealth>();
    }

    protected virtual void LoadXPOrbContainer()
    {
        if (xpOrbContainer != null) return;

        if (cachedXpOrbContainer != null)
        {
            xpOrbContainer = cachedXpOrbContainer;
            return;
        }

        GameObject containerObject = GameObject.Find("RuntimeXPOrbs");

        if (containerObject != null)
        {
            cachedXpOrbContainer = containerObject.transform;
            xpOrbContainer = cachedXpOrbContainer;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        LoadEnemyHealth();
        LoadXPOrbContainer();
        SubscribeEvent();
    }

    protected override void OnDisable()
    {
        UnsubscribeEvent();

        base.OnDisable();
    }

    protected virtual void SubscribeEvent()
    {
        if (enemyHealth == null) return;

        enemyHealth.Died -= HandleEnemyDied;
        enemyHealth.Died += HandleEnemyDied;
    }

    protected virtual void UnsubscribeEvent()
    {
        if (enemyHealth == null) return;

        enemyHealth.Died -= HandleEnemyDied;
    }

    protected virtual void HandleEnemyDied()
    {
        DropXP();
    }

    public void ConfigureDropBurst(int count, float scatterRadius = 1.15f)
    {
        dropCount = Mathf.Max(1, count);
        burstScatterRadius = Mathf.Max(0f, scatterRadius);
    }

    protected virtual void DropXP()
    {
        if (xpOrbPrefab == null) return;

        LoadXPOrbContainer();

        Vector3 centerPosition = transform.position;

        // Trường hợp 1: Quái thường chỉ rơi 1 hạt duy nhất -> Sinh ngay tại tâm (0 tính toán phụ)
        if (dropCount <= 1 || burstScatterRadius <= 0.01f)
        {
            PracticeObjectPoolManager.SpawnOrInstantiate(
                xpOrbPrefab,
                centerPosition,
                Quaternion.identity,
                xpOrbContainer
            );
            return;
        }

        // Trường hợp 2: Quái Tinh Anh (Elite) hoặc Mid-Boss -> Nổ ra chùm hạt XP tỏa tròn 360 độ
        float angleStep = 360f / dropCount;
        float randomStartAngle = Random.Range(0f, 360f);

        for (int i = 0; i < dropCount; i++)
        {
            float angleDeg = randomStartAngle + (angleStep * i);
            float angleRad = angleDeg * Mathf.Deg2Rad;

            // Khoảng cách ngẫu nhiên từ 35% đến 100% bán kính để chùm hạt nhìn tự nhiên không bị cứng nhắc
            float distance = Random.Range(burstScatterRadius * 0.35f, burstScatterRadius);//chia đều xp rơi
            Vector3 offset = new Vector3(
                Mathf.Cos(angleRad) * distance,
                Mathf.Sin(angleRad) * distance,
                0f
            );

            PracticeObjectPoolManager.SpawnOrInstantiate(
                xpOrbPrefab,
                centerPosition + offset,
                Quaternion.identity,
                xpOrbContainer
            );
        }
    }
}