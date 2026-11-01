using UnityEngine;

public class PracticeShooterWithDamage : HaiMonoBehaviour
{
    [SerializeField]
    protected PracticeBulletMovement bulletPrefab;

    [SerializeField]
    protected Transform firePoint;

    [SerializeField]
    protected Transform bulletContainer;

    [SerializeField]
    protected float shootInterval = 0.5f;

    [SerializeField]
    protected int bulletDamage = 10;

    [SerializeField]
    protected int projectileCount = 1;

    [SerializeField]
    protected float projectileSpreadAngle = 12f;

    protected float nextShootTime;

    public int BulletDamage => bulletDamage;
    public float ShootInterval => shootInterval;
    public int ProjectileCount => projectileCount;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadBulletContainer();
    }

    protected override void Awake()
    {
        base.Awake();
        LoadBulletContainer();
        nextShootTime = Time.time;
    }

    protected override void ResetValues()
    {
        base.ResetValues();
        shootInterval = 0.5f;
        projectileCount = 1;
        projectileSpreadAngle = 12f;
    }

    private void LoadBulletContainer()
    {
        if (bulletContainer != null)
        {
            return;
        }

        GameObject containerObj = GameObject.Find("RuntimePlayerBullets");

        if (containerObj != null)
        {
            bulletContainer = containerObj.transform;
        }
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
        if (bulletPrefab == null || firePoint == null || Time.time < nextShootTime)
        {
            return false;
        }

        return true;
    }

    protected virtual PracticeEnemyHealth FindTarget()
    {
        return PracticeNearestEnemyFinder.FindNearest(firePoint.position);
    }

    protected void ShootAt(PracticeEnemyHealth target)
    {
        Vector2 spawnPosition = firePoint.position;
        Vector2 baseDirection = (target.transform.position - firePoint.position).normalized;

        if (projectileCount <= 1)
        {
            SpawnSingleProjectile(spawnPosition, baseDirection);
        }
        else
        {
            float startAngle = -projectileSpreadAngle * (projectileCount - 1) * 0.5f;

            for (int i = 0; i < projectileCount; i++)
            {
                float currentAngle = startAngle + projectileSpreadAngle * i;
                Vector2 direction = RotationDirection(baseDirection, currentAngle);

                SpawnSingleProjectile(spawnPosition, direction);
            }
        }

        // Chỉ phát âm thanh bắn đúng 1 lần cho mỗi loạt đạn (dù bắn 1 viên hay chùm nhiều viên)
        PracticeAudioManager.Instance?.PlayShootSfx();

        nextShootTime = Time.time + shootInterval;
    }

    private void SpawnSingleProjectile(Vector2 spawnPosition, Vector2 baseDirection)
    {
        LoadBulletContainer();

        PracticeBulletMovement bullet =
            PracticeObjectPoolManager.SpawnOrInstantiate(
                bulletPrefab,
                spawnPosition,
                Quaternion.identity,
                bulletContainer
            );//giúp rút đạn từ kho Pool đã tạo sẵn với 0 byte cấp phát rác 

        if (bullet == null)
        {
            return;
        }

        bullet.SetDirection(baseDirection);

        PracticeBulletDamageDealer dealer =
            bullet.GetComponent<PracticeBulletDamageDealer>();

        if (dealer != null)
        {
            dealer.SetDamage(bulletDamage);
        }
    }

    private Vector2 RotationDirection(Vector2 baseDirection, float currentAngle)
    {
        Quaternion rotation = Quaternion.Euler(0f, 0f, currentAngle);
        return rotation * baseDirection;
    }

    public void AddBulletDamage(int amount)
    {
        if (amount <= 0) return;

        bulletDamage += amount;
    }

    public void ReduceShootInterval(float amount)
    {
        if (amount <= 0) return;

        shootInterval = Mathf.Max(0.1f, shootInterval - amount);
    }

    public void AddProjectileCount(int amount)
    {
        if (amount <= 0) return;

        projectileCount += amount;
    }
}