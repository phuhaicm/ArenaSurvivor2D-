using System;
using UnityEngine;

public class PracticeEnemyHealth : PracticeHealthBase, IPracticePoolable
{
    [Header("Target Classification")]
    [Tooltip("Tick chọn trên Prefab của Final Boss để súng ưu tiên khóa mục tiêu vào Boss.")]
    [SerializeField]
    private bool isBossTarget;

    [Header("Components")]
    [SerializeField]
    private Collider2D enemyCollider;

    [SerializeField]
    private Rigidbody2D enemyRigidbody;

    private float baseMaxHealth;
    private bool hasCachedBaseMaxHealth;
    private bool isInvulnerable;

    // Cho phép BossAttackController chặn cái chết khi Boss vẫn còn Phase máu tiếp theo
    public Func<bool> LethalDamageInterceptor { get; set; }

    public bool IsBossTarget => isBossTarget;
    public bool IsInvulnerable => isInvulnerable;
    public float BaseMaxHealth => hasCachedBaseMaxHealth ? baseMaxHealth : maxHealth;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadPhysicsComponents();
    }

    protected override void Awake()
    {
        CacheBaseMaxHealthIfNeeded();
        base.Awake();
        LoadPhysicsComponents();
    }

    protected override void OnEnable()
    {
        CacheBaseMaxHealthIfNeeded();
        base.OnEnable();
        LoadPhysicsComponents();
        ResetRuntimeState();

        PracticeTargetRegistry.RegisterTarget(this);
    }

    protected override void OnDisable()
    {
        PracticeTargetRegistry.UnregisterTarget(this);
        base.OnDisable();
    }

    private void CacheBaseMaxHealthIfNeeded()
    {
        if (hasCachedBaseMaxHealth)
        {
            return;
        }

        baseMaxHealth = Mathf.Max(1f, maxHealth);
        hasCachedBaseMaxHealth = true;
    }

    private void LoadPhysicsComponents()
    {
        if (enemyCollider == null)
        {
            enemyCollider = GetComponent<Collider2D>();
        }

        if (enemyRigidbody == null)
        {
            enemyRigidbody = GetComponent<Rigidbody2D>();
        }
    }

    public void SetInvulnerable(bool invulnerable)
    {
        isInvulnerable = invulnerable;
    }

    public void ConfigureMaxHealthAndRefill(float newMaxHealth)
    {
        maxHealth = Mathf.Max(1f, newMaxHealth);
        currentHealth = maxHealth;
        isDead = false;

        NotifyHealthChanged();
    }

    public override void TakeDamage(float damage)
    {
        if (isDead || isInvulnerable || damage <= 0f)
        {
            return;
        }

        base.TakeDamage(damage);
    }

    protected override void Die()
    {
        if (isDead)
        {
            return;
        }

        // Nếu Boss còn Phase tiếp theo, LethalDamageInterceptor sẽ trả về true:
        // Chuyển sang Phase mới, nạp lại đầy cây máu mới và KHÔNG gọi chết Boss!
        if (LethalDamageInterceptor != null && LethalDamageInterceptor.Invoke())
        {
            return;
        }

        PracticeTargetRegistry.UnregisterTarget(this);

        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        base.Die();

        PracticeObjectPoolManager.DespawnOrDestroy(gameObject);
    }

    public void OnTakenFromPool()
    {
        ResetRuntimeState();
        PracticeTargetRegistry.RegisterTarget(this);
    }

    public void OnReturnedToPool()
    {
        PracticeTargetRegistry.UnregisterTarget(this);
        isInvulnerable = false;

        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        StopPhysicsVelocity();
    }

    public void ResetRuntimeState()
    {
        CacheBaseMaxHealthIfNeeded();
        isInvulnerable = false;
        maxHealth = baseMaxHealth;
        ResetHealth();

        if (enemyCollider != null)
        {
            enemyCollider.enabled = true;
        }

        StopPhysicsVelocity();
    }

    private void StopPhysicsVelocity()
    {
        if (enemyRigidbody == null)
        {
            return;
        }

        enemyRigidbody.velocity = Vector2.zero;
        enemyRigidbody.angularVelocity = 0f;
    }
}