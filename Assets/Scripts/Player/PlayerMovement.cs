using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerMovement : HaiMonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Bounds")]
    [SerializeField] private Collider2D playAreaBounds;

    private Rigidbody2D rb;
    private Collider2D playerCollider;

    private Vector2 moveInput;
    private Vector2 moveDirection;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadRigidbody();
        LoadPlayerCollider();
        LoadPlayAreaBounds();
    }

    protected override void ResetValues()
    {
        base.ResetValues();
        moveSpeed = 5f;
    }

    protected void Update()
    {
        ReadInput();
    }

    protected void FixedUpdate()
    {
        Move();
        ClampInsideBounds();
    }

    protected virtual void LoadRigidbody()
    {
        if (rb != null) return;

        rb = GetComponent<Rigidbody2D>();
        LogLoad(nameof(LoadRigidbody));
    }

    protected virtual void LoadPlayerCollider()
    {
        if (playerCollider != null) return;

        playerCollider = GetComponent<Collider2D>();
        LogLoad(nameof(LoadPlayerCollider));
    }

    protected virtual void LoadPlayAreaBounds()
    {
        if (playAreaBounds != null) return;

        PlayAreaBoundsMarker boundsMarker = FindFirstObjectByType<PlayAreaBoundsMarker>();

        if (boundsMarker == null)
        {
            Debug.LogWarning($"{name}: PlayAreaBoundsMarker not found.", gameObject);
            return;
        }

        playAreaBounds = boundsMarker.GetComponent<Collider2D>();

        if (playAreaBounds == null)
        {
            Debug.LogError($"{name}: PlayAreaBoundsMarker is missing Collider2D.", boundsMarker.gameObject);
            return;
        }

        LogLoad(nameof(LoadPlayAreaBounds));
    }

    protected void ReadInput()
    {
        moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        moveDirection = moveInput.normalized;
    }

    protected void Move()
    {
        if (rb == null) return;

        rb.velocity = moveDirection * moveSpeed;
    }

    protected void ClampInsideBounds()
    {
        if (rb == null || playerCollider == null || playAreaBounds == null)
        {
            return;
        }

        Bounds areaBounds = playAreaBounds.bounds;
        Bounds currentPlayerBounds = playerCollider.bounds;

        float halfPlayerWidth = currentPlayerBounds.extents.x;
        float halfPlayerHeight = currentPlayerBounds.extents.y;

        Vector2 clampedPosition = rb.position;

        clampedPosition.x = Mathf.Clamp(
            clampedPosition.x,
            areaBounds.min.x + halfPlayerWidth,
            areaBounds.max.x - halfPlayerWidth
        );

        clampedPosition.y = Mathf.Clamp(
            clampedPosition.y,
            areaBounds.min.y + halfPlayerHeight,
            areaBounds.max.y - halfPlayerHeight
        );

        rb.position = clampedPosition;
    }
}