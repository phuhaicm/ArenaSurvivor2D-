using UnityEngine;

public class PlayerBulletMovement : HaiMonoBehaviour
{
    [SerializeField] private float moveSpeed = 12f;

    private Vector2 moveDirection;

    protected override void ResetValues()
    {
        base.ResetValues();
        moveSpeed = 12f;
    }

    private void Update()
    {
        transform.position += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);
    }

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void SetDamage(int damageAmount)
    {
        PlayerBulletDamageDealer dealer = GetComponent<PlayerBulletDamageDealer>();
        if (dealer != null)
        {
            dealer.SetDamage(damageAmount);
        }
    }
}