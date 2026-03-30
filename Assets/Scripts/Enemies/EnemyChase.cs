using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyChase : HaiMonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stoppingDistance = 0.6f;
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

    private void LoadRigidbody()
    {
        if (rb != null) return;
        rb = GetComponent<Rigidbody2D>();
        LogLoad(nameof(LoadRigidbody));
    }

    private void LoadTarget()
    {
        if (target != null) return;

        PlayerRoot playerRoot = FindFirstObjectByType<PlayerRoot>();
        if (playerRoot == null) return;

        target = playerRoot.transform;
        LogLoad(nameof(LoadTarget));
    }

    private bool HasTarget() => target != null;

    private void UpdateMoveDirection()
    {
        Vector2 directionToTarget = (Vector2)target.position - rb.position;
        float distance = directionToTarget.magnitude;
        moveDirection = distance <= stoppingDistance ? Vector2.zero : directionToTarget.normalized;
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