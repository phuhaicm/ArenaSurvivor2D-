using UnityEngine;

public class PracticeBulletMovement : HaiMonoBehaviour, IPracticePoolable
{
    [SerializeField]
    private float moveSpeed = 12f;

    private Vector2 moveDirection;

    protected override void ResetValues()
    {
        base.ResetValues();
        moveSpeed = 12f;
    }

    private void Update()
    {
        transform.position += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);
        //bay thẳng theo hướng đã chọn
    }

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        //đổi vector hướng thành góc độ phải là 0, lên trên  là 90 trái là 180 toán học góc xoay

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void OnTakenFromPool()
    {
    }

    public void OnReturnedToPool()
    {
        moveDirection = Vector2.zero;
    }

    public void ResetRuntimeState()
    {
        moveDirection = Vector2.zero;
    }
}