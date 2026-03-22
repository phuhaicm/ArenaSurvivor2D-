using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : HaiMonoBehaviour
{
    [Header("Move Settings")]
    [SerializeField] protected float moveSpeed = 5f;

    [Header("Bounds")]
    [SerializeField] protected Collider2D playAreaBounds;

    protected Rigidbody2D rb;
    protected Collider2D playerCollider;

    protected Vector2 moveInput;
    protected Vector2 moveDirection;

    protected void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
    }

    protected void Update()
    {
        ReadInput();
    }
    protected void ReadInput()
    {
        moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
            );
        moveDirection = moveInput.normalized;
    }
    protected void FixedUpdate()
    {
        Move();
        ClapInsideBounds();
    }
    protected void Move()
    {
        rb.velocity = moveDirection * moveSpeed;
    }
    protected void ClapInsideBounds()
    {
        if (playAreaBounds == null || playerCollider == null) return;
        Bounds areaBounds = playAreaBounds.bounds;
        Bounds curretnPlayerBounds = playerCollider.bounds;

        float halfPlayerWidth = curretnPlayerBounds.extents.x;
        float halfPlayerHeight = curretnPlayerBounds.extents.y;

        Vector2 clampedPosition = rb.position;

        clampedPosition.x = Mathf.Clamp(
            clampedPosition.x,
            areaBounds.min.x + halfPlayerHeight,
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
