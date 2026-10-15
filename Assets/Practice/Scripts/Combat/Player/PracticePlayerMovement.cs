using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PracticePlayerMovement : HaiMonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField, Min(0f)]
    private float moveSpeed = 5f;

    [Header("Bounds Settings")]
    [SerializeField] private float minX = -8f;
    [SerializeField] private float maxX = 8f;
    [SerializeField] private float minY = -4f;
    [SerializeField] private float maxY = 4f;
    private bool canMove = true;
    private Rigidbody2D playerRigidbody;
    private Collider2D playerCollider;
    private Vector2 movementInput;

    protected override void LoadComponents()
{
    base.LoadComponents();

    playerRigidbody = GetComponent<Rigidbody2D>();
    playerCollider = GetComponent<Collider2D>();
}

    private void Update()
    {
        if(!canMove) return;
        ReadMovementInput();
    }

    private void FixedUpdate()
    {
        if(!canMove) return;
        Move();
    }

    private void ReadMovementInput()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        movementInput = new Vector2(
            horizontalInput,
            verticalInput
        ).normalized;
    }

    private void Move()
    {
        Vector2 displacement = movementInput * moveSpeed * Time.fixedDeltaTime;

        Vector2 targetPosition = playerRigidbody.position + displacement;
        
        Vector2 clampedPosition = ClampPositionToBounds(targetPosition);

        playerRigidbody.MovePosition(clampedPosition);
    }

    protected Vector2 ClampPositionToBounds(Vector2 targetPosition)
    {
        Vector2 colliderExtents = playerCollider.bounds.extents;
        
        float allowedMinX = minX + colliderExtents.x;
        float allowedMaxX = maxX - colliderExtents.x;
        float allowedMinY = minY + colliderExtents.y;
        float allowedMaxY = maxY - colliderExtents.y;

        if(!HasvalidMovementArea(allowedMinX, allowedMaxX, allowedMinY, allowedMaxY))
        {
            Debug.LogWarning("Player movement area is invalid. Please check the bounds settings.");
            return playerRigidbody.position;
        }

        float clampedX = Mathf.Clamp(targetPosition.x, allowedMinX, allowedMaxX);
        float clampedY = Mathf.Clamp(targetPosition.y, allowedMinY, allowedMaxY);

        return new Vector2(clampedX, clampedY);
    }
    protected bool HasvalidMovementArea(float allowedMinX, float allowedMaxX, float allowedMinY, float allowedMaxY)
    {
        return allowedMinX < allowedMaxX && allowedMinY < allowedMaxY;
    }

    protected void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 boundsCenter = new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, 0f);

        Vector3 boundsSize = new Vector3(maxX - minX, maxY - minY, 0f);
        Gizmos.DrawWireCube(boundsCenter, boundsSize);
    }
    public void SetMoveEnable(bool isEnabled)
    {
        canMove = isEnabled;

        if (!canMove)
        {
            movementInput = Vector2.zero;// xóa input còn dư
        }
    }
}