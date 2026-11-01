using UnityEngine;

public class PracticeEnemyHealth : PracticeHealthBase, IPracticePoolable
{
    [Header("Components")]
    [SerializeField]
    private Collider2D enemyCollider;

    [SerializeField]
    private Rigidbody2D enemyRigidbody;

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
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        // Gọi base.Die() trước để đặt isDead = true và phát sự kiện Died
        // cho Spawner, KillCounter, XPDropOnDeath và BossController nhận tín hiệu
        base.Die();

        // Thu hồi về Object Pool thay vì Destroy(gameObject)
        PracticeObjectPoolManager.DespawnOrDestroy(gameObject);
    }

    public void OnTakenFromPool()
    {
        ResetRuntimeState();
    }

    public void OnReturnedToPool()
    {
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        StopPhysicsVelocity();
    }

    public void ResetRuntimeState()
    {
        // Gọi ResetHealth() từ PracticeHealthBase để đặt currentHealth = maxHealth và isDead = false
        ResetHealth();//để nạp đầy bình máu

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