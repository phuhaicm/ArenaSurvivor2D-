using UnityEngine;

public class PlayerAutoShooter : HaiMonoBehaviour
{
    [SerializeField] private float shootInterval = 0.6f;
    [SerializeField] private int bulletDamage = 20;
    [SerializeField] private int projectileCount = 1;
    [SerializeField] private float projectileSpreadAngle = 12f;

    private WeaponMuzzleRoot weaponMuzzleRoot;
    private EnemyContainerRoot enemyContainerRoot;
    private BulletContainerRoot bulletContainerRoot;
    private PlayerHealth playerHealth;

    private float nextShootTime;

    protected override void Awake()
    {
        base.Awake();
        nextShootTime = Time.time + shootInterval;
    }

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadWeaponMuzzleRoot();
        LoadEnemyContainerRoot();
        LoadBulletContainerRoot();
        LoadPlayerHealth();
    }

    protected override void ResetValues()
    {
        base.ResetValues();
        shootInterval = 0.6f;
        bulletDamage = 20;
        projectileCount = 1;
        projectileSpreadAngle = 12f;
    }

    private void Update()
    {
        if (!CanShoot())
        {
            return;
        }

        TryShootNearestEnemy();
    }

    private void LoadWeaponMuzzleRoot()
    {
        if (weaponMuzzleRoot != null) return;
        weaponMuzzleRoot = GetComponentInChildren<WeaponMuzzleRoot>(true);
    }

    private void LoadEnemyContainerRoot()
    {
        if (enemyContainerRoot != null) return;
        enemyContainerRoot = FindFirstObjectByType<EnemyContainerRoot>();
    }

    private void LoadBulletContainerRoot()
    {
        if (bulletContainerRoot != null) return;
        bulletContainerRoot = FindFirstObjectByType<BulletContainerRoot>();
    }

    private void LoadPlayerHealth()
    {
        if (playerHealth != null) return;
        playerHealth = GetComponentInParent<PlayerHealth>();
    }

    private bool CanShoot()
    {
        if (weaponMuzzleRoot == null || enemyContainerRoot == null) return false;
        if (playerHealth != null && playerHealth.IsDead) return false;
        if (Time.time < nextShootTime) return false;
        return true;
    }

    private void TryShootNearestEnemy()
    {
        Vector2 shootOrigin = weaponMuzzleRoot.transform.position;
        EnemyHealth target = EnemyTargetFinder.FindNearestAliveEnemy(shootOrigin, enemyContainerRoot.transform);

        if (target == null)
        {
            return;
        }

        SpawnProjectilesTowards(target.transform.position);
        nextShootTime = Time.time + shootInterval;
    }

    private void SpawnProjectilesTowards(Vector2 targetPosition)
    {
        Vector2 spawnPosition = weaponMuzzleRoot.transform.position;
        Vector2 baseDirection = (targetPosition - spawnPosition).normalized;

        if (projectileCount <= 1)
        {
            SpawnSingleProjectile(spawnPosition, baseDirection);
            return;
        }

        float startAngle = -projectileSpreadAngle * (projectileCount - 1) * 0.5f;

        for (int i = 0; i < projectileCount; i++)
        {
            float currentAngle = startAngle + projectileSpreadAngle * i;
            Vector2 direction = RotateDirection(baseDirection, currentAngle);
            SpawnSingleProjectile(spawnPosition, direction);
        }
    }

    private void SpawnSingleProjectile(Vector2 spawnPosition, Vector2 direction)
    {
        PlayerBulletMovement bulletPrefab = ResourcePrefabLoader.LoadPrefab<PlayerBulletMovement>(GameResourcePaths.PlayerBullet);
        if (bulletPrefab == null) return;

        Transform parent = bulletContainerRoot != null ? bulletContainerRoot.transform : null;
        PlayerBulletMovement bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity, parent);
        bullet.SetDirection(direction);
        bullet.SetDamage(bulletDamage);
    }

    private Vector2 RotateDirection(Vector2 direction, float angleDegrees)
    {
        Quaternion rotation = Quaternion.Euler(0f, 0f, angleDegrees);
        return rotation * direction;
    }

    public void AddBulletDamage(int amount)
    {
        if (amount <= 0) return;
        bulletDamage += amount;
    }

    public void ReduceShootInterval(float amount)
    {
        if (amount <= 0f) return;
        shootInterval = Mathf.Max(0.1f, shootInterval - amount);
    }

    public void AddProjectileCount(int amount)
    {
        if (amount <= 0) return;
        projectileCount += amount;
    }
}