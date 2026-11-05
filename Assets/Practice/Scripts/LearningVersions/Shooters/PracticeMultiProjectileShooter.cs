using UnityEngine;

public class PracticeAutoShooterSimple : HaiMonoBehaviour
{
    [SerializeField] protected PracticeBulletMovement bulletPrefab;
    [SerializeField] protected Transform firePoint;
    [SerializeField] protected float shootInterval = 0.5f;

    [SerializeField] protected int projectileCount = 1;
    [SerializeField] protected float projectileSpreadAngle = 12f;

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
        projectileCount = 1;
        projectileSpreadAngle = 12f;

    }

    private void Update()
    {
        if (!CanShoot()) return;

        PracticeEnemyHealth target = FindTarget();
        if (target == null) return;

        ShootAt(target);
    }

    protected virtual bool CanShoot()
    {
        if (bulletPrefab == null || firePoint == null || Time.time < nextShootTime) return false;

        return true;
    }

    protected virtual PracticeEnemyHealth FindTarget()
    {
        return PracticeNearestEnemyFinder.FindNearest(firePoint.position);
        //hàm này có trách nhiệm trả về object có nghĩa là tìm enemy và trả enemy cho hàm findtarget    
    }

    protected void ShootAt(PracticeEnemyHealth target)
    {
        Vector2 spawnPosition = firePoint.position;
        Vector2 baseDirection = (target.transform.position - firePoint.position).normalized;

        if (projectileCount <= 1)
        {
            SpawnSingleProjectile(spawnPosition,baseDirection);
            nextShootTime = Time.time + shootInterval;
            return;
        }

        float startAngle = -projectileSpreadAngle * (projectileCount - 1) * 0.5f;

        for (int i = 0; i < projectileCount; i++) 
        {
            float currentAngle = startAngle + projectileSpreadAngle * i;
            Vector2 direction = RotationDirection(baseDirection, currentAngle);

            SpawnSingleProjectile(spawnPosition, direction);
        }

        nextShootTime = Time.time + shootInterval;
    }

    private void SpawnSingleProjectile(Vector2 spawnPosition, Vector2 baseDirection)
    {
        PracticeBulletMovement bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);

        bullet.SetDirection(baseDirection);

    }

    private Vector2 RotationDirection(Vector2 baseDirection, float currentAngle)
    {
        Quaternion rotation = Quaternion.Euler(0f, 0f, currentAngle);
        return rotation * baseDirection;
    }
}
