using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyChase : HaiMonoBehaviour
{
    [Header("Chase Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stoppingDistance = 0.6f;

    [Header("Target")]
    [SerializeField] private Transform target;

    private Rigidbody2D rb;
    private Vector2 moveDirection;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadRigidbody();
        LoadTarget();
    }

    protected override void ResetValues()
    {
        base.ResetValues();
        moveSpeed = 3f;
        stoppingDistance = 0.6f;
    }

    private void FixedUpdate()
    {
        if (!HasTarget())
        {
            StopMoving();
            return;
        }

        UpdateMoveDirection();
        Move();
    }

    protected virtual void LoadRigidbody()
    {
        if (rb != null) return;

        rb = GetComponent<Rigidbody2D>();
        LogLoad(nameof(LoadRigidbody));
    }

    protected virtual void LoadTarget()
    {
        if (target != null) return;

        PlayerRoot playerRoot = FindFirstObjectByType<PlayerRoot>();

        if (playerRoot == null)
        {
            Debug.LogWarning($"{name}: PlayerRoot not found.", gameObject);
            return;
        }

        target = playerRoot.transform;
        LogLoad(nameof(LoadTarget));
    }

    private bool HasTarget()
    {
        return target != null;
    }

    private void UpdateMoveDirection()
    {
        Vector2 currentPosition = rb.position;
        Vector2 targetPosition = target.position;

        Vector2 directionToTarget = targetPosition - currentPosition;
        float distanceToTarget = directionToTarget.magnitude;

        if (distanceToTarget <= stoppingDistance)
        {
            moveDirection = Vector2.zero;
            return;
        }

        moveDirection = directionToTarget.normalized;
    }

    private void Move()
    {
        if (rb == null) return;

        rb.velocity = moveDirection * moveSpeed;
    }

    private void StopMoving()
    {
        if (rb == null) return;

        rb.velocity = Vector2.zero;
    }
}