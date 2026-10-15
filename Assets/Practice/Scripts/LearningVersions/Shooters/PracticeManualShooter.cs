using UnityEngine;

public class PracticeManualShooter : HaiMonoBehaviour
{
    [SerializeField] protected PracticeBulletMovement bulletPrefab;
    [SerializeField] protected Transform firePoint;
    [SerializeField] protected Vector2 shootDirection = Vector2.right;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space pressed - Shoot called", gameObject);
            Shoot();
        }
    }

    protected virtual void Shoot()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("Bullet Prefab is null", gameObject);
            return;
        }

        if (firePoint == null)
        {
            Debug.LogWarning("Fire Point is null", gameObject);
            return;
        }

        PracticeBulletMovement bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.identity
        );

        bullet.SetDirection(shootDirection);

        Debug.Log("Bullet spawned", bullet.gameObject);
    }
}
