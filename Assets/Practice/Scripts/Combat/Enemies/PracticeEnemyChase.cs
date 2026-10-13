using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PracticeEnemyChase : HaiMonoBehaviour
{
    [Header("Chase Settings")]  
    [SerializeField, Min(0f)]

    private float moveSpeed = 2f;
    [SerializeField, Min(0f)]
    private float stoppingDistance = 0.8f;
    [Header("Target Settings")]
    [SerializeField, Min(0.1f)]
    private float targetSearchInterval = 0.5f;
    private Rigidbody2D enemyRigidbody;
    private Transform target;
    private float nextTargetSearchTime;
    //ko cho bằng 0 nhưng khi chạy là bằng 0 hả
    protected override void LoadComponents()
    {
        base.LoadComponents();

        enemyRigidbody = GetComponent<Rigidbody2D>();
    } 
    protected override void Start()
    {
        base.Start();
        FindTarget();
    }
    protected void FixedUpdate()
    {
        if(target == null)
        {
            TryFindTarget();
            return;
        }

        ChaseTarget();
    }
    protected void TryFindTarget()
    {
        if(Time.time >= nextTargetSearchTime)
        {
            nextTargetSearchTime = Time.time + targetSearchInterval;
            FindTarget();
        }
    }

    protected void FindTarget()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if(playerObject != null)
        {
            target = playerObject.transform;
        }
    }
    protected void ChaseTarget()
    {
        Vector2 directionToTarget = (Vector2)target.position - enemyRigidbody.position;
        float distanceTotarget = directionToTarget.magnitude;
        if(distanceTotarget > stoppingDistance)
        {
            Vector2 moveDirection = directionToTarget.normalized;
            //nếu ko có thì enemy càng xa player thì vector càng lớn enemy sẽ chạy nhanh hơn, có thể gây ra lỗi
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
}
