
using UnityEngine;

public class PracticeShooterCooldown : HaiMonoBehaviour
{
    [SerializeField] protected PracticeBulletMovement bulletPrefab;
    [SerializeField] protected Transform firePoint;
    [SerializeField] protected Vector2 shootDirection = Vector2.right;
    [SerializeField] protected float shootInterval = 0.5f;

    protected float nextShootTime;

    protected override void Awake()
    {
        base.Awake();
        nextShootTime = Time.time;
    }

    protected override void ResetValues()
    {
        base.ResetValues();
        shootInterval = 0.5f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space pressed - Shoot called", gameObject);
            Shoot();
        }
    }

    protected virtual bool CanShoot()
    {
        if (bulletPrefab == null || firePoint == null || Time.time < nextShootTime) return false;

        return true;
    }

    protected virtual void Shoot()
    {
        if(!CanShoot()) return;

        PracticeBulletMovement bullet = Instantiate(
           bulletPrefab,
           firePoint.position,
           Quaternion.identity
       );

        bullet.SetDirection(shootDirection);

        nextShootTime = Time.time + shootInterval;
    }
}
