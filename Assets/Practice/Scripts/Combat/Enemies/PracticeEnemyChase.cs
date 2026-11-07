using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PracticeEnemyChase : HaiMonoBehaviour, IPracticePoolable
{
    // Lưu đệm Transform của Player ở cấp static để 1.000 con quái dùng chung ngay lập tức (O(1)),
    // không phải gọi FindGameObjectWithTag("Player") 1.000 lần khi khởi tạo Pool.
    private static Transform cachedPlayerTarget;

    [Header("Chase Settings")]
    [SerializeField, Min(0f)]
    private float moveSpeed = 2f;

    [SerializeField, Min(0f)]
    private float stoppingDistance = 0.8f;

    [Header("Target Settings")]
    [SerializeField, Min(0.1f)]
    private float targetSearchInterval = 0.5f;

    [Header("Components")]
    [SerializeField]
    private Rigidbody2D enemyRigidbody;

    private Transform target;
    private float nextTargetSearchTime;

    protected override void LoadComponents()
    {
        base.LoadComponents();

        if (enemyRigidbody == null)
        {
            enemyRigidbody = GetComponent<Rigidbody2D>();
        }
    }

    protected override void Awake()
    {
        base.Awake();
        LoadComponents();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        LoadComponents();
        nextTargetSearchTime = 0f;

        if (target == null)
        {
            FindTarget();
        }
    }

    protected override void Start()
    {
        base.Start();
        if (target == null)
        {
            FindTarget();
        }
    }

    protected void FixedUpdate()
    {
        if (target == null)
        {
            TryFindTarget();
            return;
        }

        ChaseTarget();
    }

    protected void TryFindTarget()
    {
        if (Time.time >= nextTargetSearchTime)
        {
            nextTargetSearchTime = Time.time + targetSearchInterval;
            FindTarget();
        }
    }

    protected void FindTarget()
    {
        if (cachedPlayerTarget != null)
        {
            target = cachedPlayerTarget;
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            cachedPlayerTarget = playerObject.transform;
            target = cachedPlayerTarget;
            return;
        }

        PracticePlayerHealth foundPlayer = FindFirstObjectByType<PracticePlayerHealth>();
        if (foundPlayer != null)
        {
            cachedPlayerTarget = foundPlayer.transform;
            target = cachedPlayerTarget;
        }
    }

    protected void ChaseTarget()
    {
        if (enemyRigidbody == null)
        {
            return;
        }

        Vector2 directionToTarget = (Vector2)target.position - enemyRigidbody.position;
        float distanceSqr = directionToTarget.sqrMagnitude;
        float stoppingDistanceSqr = stoppingDistance * stoppingDistance;

        if (distanceSqr > stoppingDistanceSqr)
        {
            float distanceToTarget = Mathf.Sqrt(distanceSqr);
            Vector2 moveDirection = directionToTarget / distanceToTarget;
            float movementThisStep = moveSpeed * Time.fixedDeltaTime;
            float maximumAllowedMovement = distanceToTarget - stoppingDistance;
            float moveDistance = Mathf.Min(movementThisStep, maximumAllowedMovement);

            Vector2 displacement = moveDirection * moveDistance;
            Vector2 nextPosition = enemyRigidbody.position + displacement;
            enemyRigidbody.MovePosition(nextPosition);
        }
    }

    protected void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
    }

    public void OnTakenFromPool()
    {
        nextTargetSearchTime = 0f;

        if (target == null)
        {
            FindTarget();
        }
    }

    public void OnReturnedToPool()
    {
        if (enemyRigidbody != null)
        {
            enemyRigidbody.velocity = Vector2.zero;//phải có cái này để reset quái hồi sinh từ pool ko tiếp tục trôi dạt về phía sau theo quán tính nữa

            enemyRigidbody.angularVelocity = 0f;
        }
    }

    public void ResetRuntimeState()
    {
        nextTargetSearchTime = 0f;
    }
}
