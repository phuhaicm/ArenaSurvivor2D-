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

    public bool IsBossTarget => isBossTarget;//Đánh dấu đây có phải là mục tiêu ưu tiên cấp Boss hay không.

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadPhysicsComponents();
    }

    protected override void Awake()
    {
        base.Awake();
        LoadPhysicsComponents();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        LoadPhysicsComponents();
        ResetRuntimeState();

        // Quái vừa bật lên (sinh mới hoặc lấy từ Pool) -> Tự động ghi tên vào Target Registry
        PracticeTargetRegistry.RegisterTarget(this);
    }

    protected override void OnDisable()
    {
        // Quái bị tắt (trả về Pool hoặc đổi Scene) -> Tự động rút tên khỏi Target Registry
        PracticeTargetRegistry.UnregisterTarget(this);

        base.OnDisable();
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

    protected override void Die()
    {
        // Rút tên khỏi Registry ngay lập tức khi máu về 0 để viên đạn tiếp theo không bắn vào xác quái
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

        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        StopPhysicsVelocity();
    }

    public void ResetRuntimeState()
    {
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