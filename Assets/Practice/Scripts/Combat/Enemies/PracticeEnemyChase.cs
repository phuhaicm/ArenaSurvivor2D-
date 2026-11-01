using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PracticeEnemyChase : HaiMonoBehaviour, IPracticePoolable
{
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
        FindTarget();
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
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            target = playerObject.transform;
        }
    }

    protected void ChaseTarget()
    {
        if (enemyRigidbody == null)
        {
            return;
        }

        Vector2 directionToTarget = (Vector2)target.position - enemyRigidbody.position;
        float distanceTotarget = directionToTarget.magnitude;

        if (distanceTotarget > stoppingDistance)
        {
            Vector2 moveDirection = directionToTarget.normalized;
            float movementThisStep = moveSpeed * Time.fixedDeltaTime;
            float maximumAllowedMovement = distanceTotarget - stoppingDistance;
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
            enemyRigidbody.velocity = Vector2.zero;
            enemyRigidbody.angularVelocity = 0f;
        }
    }

    public void ResetRuntimeState()
    {
        nextTargetSearchTime = 0f;
    }
}