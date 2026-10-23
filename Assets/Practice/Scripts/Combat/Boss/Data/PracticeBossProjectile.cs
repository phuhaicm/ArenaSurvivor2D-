using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PracticeBossProjectile
    : HaiMonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Rigidbody2D projectileRigidbody;

    [SerializeField]
    private Collider2D projectileCollider;

    [Header("Runtime")]
    [SerializeField]
    private float damage;

    [SerializeField]
    private float lifetime;

    private Vector2 moveDirection;
    private float moveSpeed;
    private float despawnTime;
    private bool hasHitPlayer;
    private bool isInitialized;

    protected override void LoadComponents()
    {
        base.LoadComponents();

        LoadRigidbody();
        LoadCollider();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        ResetRuntimeState();
    }

    private void Update()
    {
        CheckLifetime();
    }

    private void FixedUpdate()
    {
        MoveProjectile();
    }

    private void OnTriggerEnter2D(
        Collider2D other
    )
    {
        if (!isInitialized ||
            hasHitPlayer)
        {
            return;
        }

        PracticePlayerHealth playerHealth =
            other.GetComponentInParent<
                PracticePlayerHealth
            >();

        if (playerHealth == null)
        {
            return;
        }

        hasHitPlayer = true;

        playerHealth.TakeDamage(damage);

        DestroyProjectile();
    }

    private void LoadRigidbody()
    {
        if (projectileRigidbody != null)
        {
            return;
        }

        projectileRigidbody =
            GetComponent<Rigidbody2D>();
    }

    private void LoadCollider()
    {
        if (projectileCollider != null)
        {
            return;
        }

        projectileCollider =
            GetComponent<Collider2D>();
    }

    public void Initialize(
        Vector2 direction,
        float speed,
        float damageAmount,
        float projectileLifetime
    )
    {
        moveDirection =
            direction.normalized;

        moveSpeed =
            Mathf.Max(0f, speed);

        damage =
            Mathf.Max(0f, damageAmount);

        lifetime =
            Mathf.Max(
                0.1f,
                projectileLifetime
            );

        despawnTime =
            Time.time + lifetime;

        hasHitPlayer = false;
        isInitialized = true;

        if (projectileCollider != null)
        {
            projectileCollider.enabled = true;
        }
    }

    private void MoveProjectile()
    {
        if (!isInitialized ||
            projectileRigidbody == null)
        {
            return;
        }

        Vector2 displacement =
            moveDirection *
            moveSpeed *
            Time.fixedDeltaTime;

        Vector2 nextPosition =
            projectileRigidbody.position +
            displacement;

        projectileRigidbody.MovePosition(
            nextPosition
        );
    }

    private void CheckLifetime()
    {
        if (!isInitialized)
        {
            return;
        }

        if (Time.time < despawnTime)
        {
            return;
        }

        DestroyProjectile();
    }

    private void DestroyProjectile()
    {
        isInitialized = false;

        if (projectileCollider != null)
        {
            projectileCollider.enabled = false;
        }

        Destroy(gameObject);
    }

    private void ResetRuntimeState()
    {
        moveDirection = Vector2.zero;
        moveSpeed = 0f;
        damage = 0f;
        lifetime = 0f;
        despawnTime = 0f;
        hasHitPlayer = false;
        isInitialized = false;

        if (projectileRigidbody != null)
        {
            projectileRigidbody.velocity =
                Vector2.zero;

            projectileRigidbody.angularVelocity =
                0f;
        }

        if (projectileCollider != null)
        {
            projectileCollider.enabled = true;
        }
    }
}