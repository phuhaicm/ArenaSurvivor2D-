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

    [Header("Ship Aiming & Rotation")]
    [Tooltip("Tự động xoay mũi tàu và nòng súng về hướng quái gần nhất khi có quái, hoặc theo hướng di chuyển WASD.")]
    [SerializeField]
    protected bool autoRotateShip = true;

    [Tooltip("Tốc độ xoay đầu tàu (độ/giây) đảm bảo phản hồi tức thì và mượt mà.")]
    [SerializeField, Min(180f)]
    protected float rotationSpeed = 900f;

    [SerializeField]
    protected PracticePlayerMovement playerMovement;

    protected float nextShootTime;

    public int BulletDamage => bulletDamage;
    public float ShootInterval => shootInterval;
    public int ProjectileCount => projectileCount;
    public bool AutoRotateShip => autoRotateShip;
    public float RotationSpeed => rotationSpeed;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadBulletContainer();
        LoadPlayerMovement();
    }

    protected override void Awake()
    {
        base.Awake();
        LoadBulletContainer();
        LoadPlayerMovement();
        nextShootTime = Time.time;
    }

    private void LoadPlayerMovement()
    {
        if (playerMovement != null) return;
        playerMovement = GetComponent<PracticePlayerMovement>();
    }

    protected override void ResetValues()
    {
        base.ResetValues();
        shootInterval = 0.5f;
        projectileCount = 1;
        projectileSpreadAngle = 12f;
        autoRotateShip = true;
        rotationSpeed = 900f;
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
        PracticeEnemyHealth target = FindTarget();

        if (autoRotateShip)
        {
            UpdateShipRotation(target);
        }

        if (!CanShoot()) return;
        if (target == null) return;

        ShootAt(target);
    }

    private void UpdateShipRotation(PracticeEnemyHealth target)
    {
        Vector2 aimDirection = Vector2.zero;

        // Ưu tiên 1: Hướng về kẻ địch gần nhất trong tầm ngắm
        if (target != null && !target.IsDead && target.gameObject.activeInHierarchy)
        {
            aimDirection = ((Vector2)target.transform.position - (Vector2)transform.position).normalized;
        }
        // Ưu tiên 2: Khi không có quái, hướng mũi tàu theo hướng đang lái (WASD)
        else if (playerMovement != null && playerMovement.MovementInput.sqrMagnitude > 0.01f)
        {
            aimDirection = playerMovement.MovementInput.normalized;
        }

        if (aimDirection.sqrMagnitude > 0.001f)
        {
            // Trục Y của Sprite tàu (Short-Lazer-Ship) hướng lên trên, nên trừ 90 độ để đầu tàu xoay chính xác về hướng mục tiêu
            float targetAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
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
