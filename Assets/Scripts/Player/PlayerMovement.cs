using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerMovement : HaiMonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
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

    private void Update()
    {
        ReadInput();
    }

    private void FixedUpdate()
    {
        Move();
        ClampInsideBounds();
    }

    private void LoadRigidbody()
    {
        if (rb != null) return;
        rb = GetComponent<Rigidbody2D>();
        LogLoad(nameof(LoadRigidbody));
    }

    private void LoadPlayerCollider()
    {
        if (playerCollider != null) return;
        playerCollider = GetComponent<Collider2D>();
        LogLoad(nameof(LoadPlayerCollider));
    }

    private void LoadPlayAreaBounds()
    {
        if (playAreaBounds != null) return;

        PlayAreaBoundsMarker marker = FindFirstObjectByType<PlayAreaBoundsMarker>();
        if (marker == null) return;

        playAreaBounds = marker.GetComponent<Collider2D>();
        LogLoad(nameof(LoadPlayAreaBounds));
    }

    private void ReadInput()
    {
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        moveDirection = moveInput.normalized;
    }

    private void Move()
    {
        if (rb == null) return;
        rb.velocity = moveDirection * moveSpeed;
    }

    private void ClampInsideBounds()
    {
        if (rb == null || playerCollider == null || playAreaBounds == null) return;

        Bounds area = playAreaBounds.bounds;
        Bounds current = playerCollider.bounds;

        Vector2 clamped = rb.position;
        clamped.x = Mathf.Clamp(clamped.x, area.min.x + current.extents.x, area.max.x - current.extents.x);
        clamped.y = Mathf.Clamp(clamped.y, area.min.y + current.extents.y, area.max.y - current.extents.y);

        rb.position = clamped;
    }
}