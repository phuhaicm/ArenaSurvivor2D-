using UnityEngine;

public class PracticeEnemyDamageFeedback
    : HaiMonoBehaviour
{
    // Lưu đệm Container ở cấp static để 1.000 con quái dùng chung ngay lập tức (O(1)),
    // tránh gọi GameObject.Find("RuntimeDamageTexts") lặp lại hàng trăm lần khi mở rộng Pool.
    private static Transform cachedDamageTextContainer;

    [Header("References")]
    [SerializeField]
    private PracticeEnemyHealth enemyHealth;

    [SerializeField]
    private PracticeSpriteFlashFeedback flashFeedback;

    [Header("Floating Damage Text (Optional)")]
    [SerializeField]
    private PracticeFloatingDamageText damageTextPrefab;

    [SerializeField]
    private Transform damageTextContainer;

    protected override void LoadComponents()
    {
        base.LoadComponents();

        LoadEnemyHealth();
        LoadFlashFeedback();
        LoadDamageTextContainer();
    }

    private void LoadEnemyHealth()
    {
        if (enemyHealth != null)
        {
            return;
        }

        enemyHealth =
            GetComponent<PracticeEnemyHealth>();
    }

    private void LoadFlashFeedback()
    {
        if (flashFeedback != null)
        {
            return;
        }

        flashFeedback =
            GetComponent<PracticeSpriteFlashFeedback>();
    }

    private void LoadDamageTextContainer()
    {
        if (damageTextContainer != null)
        {
            return;
        }

        if (cachedDamageTextContainer != null)
        {
            damageTextContainer = cachedDamageTextContainer;
            return;
        }

        GameObject containerObj = GameObject.Find("RuntimeDamageTexts");
        if (containerObj != null)
        {
            cachedDamageTextContainer = containerObj.transform;
            damageTextContainer = cachedDamageTextContainer;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        LoadEnemyHealth();
        LoadFlashFeedback();
        LoadDamageTextContainer();

        SubscribeEnemyEvents();
    }

    protected override void OnDisable()
    {
        UnsubscribeEnemyEvents();

        base.OnDisable();
    }

    private void SubscribeEnemyEvents()
    {
        if (enemyHealth == null)
        {
            return;
        }

        enemyHealth.DamageTaken -= HandleDamageTaken;
        enemyHealth.DamageTaken += HandleDamageTaken;

        enemyHealth.Died -= HandleEnemyDied;
        enemyHealth.Died += HandleEnemyDied;
    }

    private void UnsubscribeEnemyEvents()
    {
        if (enemyHealth == null)
        {
            return;
        }

        enemyHealth.DamageTaken -= HandleDamageTaken;
        enemyHealth.Died -= HandleEnemyDied;
    }

    private void HandleDamageTaken(float damageAmount)
    {
        flashFeedback?.PlayFlash();
        SpawnFloatingDamageText(damageAmount);

        // Phát tia lửa Hit Spark có giới hạn VFX Budget từ Object Pool
        PracticeVFXManager.Instance?.SpawnHitSpark(transform.position);
    }

    private void HandleEnemyDied()
    {
        if (PracticeVFXManager.Instance == null)
        {
            return;
        }

        if (enemyHealth != null && enemyHealth.IsBossTarget)
        {
            PracticeVFXManager.Instance.SpawnBossDeathExplosion(transform.position);
            PracticeCameraShake2D.Instance?.Shake(0.42f, 0.55f);
            return;
        }

        PracticeVFXManager.Instance.SpawnEnemyDeathBurst(transform.position);
    }

    private void SpawnFloatingDamageText(float damageAmount)
    {
        if (damageTextPrefab == null)
        {
            return;
        }

        LoadDamageTextContainer();

        PracticeFloatingDamageText spawnedText =
            PracticeObjectPoolManager.SpawnOrInstantiate(
                damageTextPrefab,
                transform.position,
                Quaternion.identity,
                damageTextContainer
            );

        if (spawnedText != null)
        {
            spawnedText.Initialize(damageAmount, transform.position);
        }
    }
}
