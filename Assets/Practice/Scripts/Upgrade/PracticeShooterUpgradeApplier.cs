using System;
using System.Collections.Generic;
using UnityEngine;

public class PracticeShooterUpgradeApplier : HaiMonoBehaviour
{
    [Header("Target Receivers")]
    [SerializeField]
    private PracticeShooterWithDamage shooter;

    [SerializeField]
    private PracticePlayerHealth playerHealth;

    [SerializeField]
    private PracticePlayerMovement playerMovement;

    [SerializeField]
    private PracticeXPMagnetDetector magnetDetector;

    private Dictionary<PracticeUpgradeType, Func<float, bool>> upgradeEffectHandlers;//là một kiểu Delegate (Con trỏ hàm) trong C# đại diện cho bất kỳ hàm nào nhận vào 1 tham số kiểu float (chính là value của nâng cấp) và trả về kết quả kiểu bool (true nếu nâng cấp thành công, false nếu thất bại).

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadReceivers();
    }

    protected override void Awake()
    {
        base.Awake();
        LoadReceivers();
        InitializeEffectHandlers();
    }

    protected void LoadReceivers()
    {
        if (playerHealth == null)
        {
            playerHealth = GetComponent<PracticePlayerHealth>();
            if (playerHealth == null)
            {
                playerHealth = GetComponentInParent<PracticePlayerHealth>();
            }
            if (playerHealth == null)
            {
                playerHealth = FindFirstObjectByType<PracticePlayerHealth>();
            }
        }

        if (shooter == null || (playerHealth != null && shooter.gameObject != playerHealth.gameObject))
        {
            if (playerHealth != null)
            {
                PracticeShooterWithDamage playerShooter =
                    playerHealth.GetComponentInChildren<PracticeShooterWithDamage>(true);
                if (playerShooter != null)
                {
                    shooter = playerShooter;
                }
            }

            if (shooter == null)
            {
                shooter = GetComponent<PracticeShooterWithDamage>();
            }
            if (shooter == null)
            {
                shooter = GetComponentInChildren<PracticeShooterWithDamage>(true);
            }
            if (shooter == null)
            {
                shooter = FindFirstObjectByType<PracticeShooterWithDamage>();
            }
        }

        if (playerMovement == null)
        {
            playerMovement = GetComponent<PracticePlayerMovement>();
            if (playerMovement == null)
            {
                playerMovement = GetComponentInParent<PracticePlayerMovement>();
            }
            if (playerMovement == null && playerHealth != null)
            {
                playerMovement = playerHealth.GetComponentInChildren<PracticePlayerMovement>(true);
            }
            if (playerMovement == null)
            {
                playerMovement = FindFirstObjectByType<PracticePlayerMovement>();
            }
        }

        if (magnetDetector == null)
        {
            magnetDetector = GetComponentInChildren<PracticeXPMagnetDetector>(true);
            if (magnetDetector == null && playerHealth != null)
            {
                magnetDetector = playerHealth.GetComponentInChildren<PracticeXPMagnetDetector>(true);
            }
            if (magnetDetector == null)
            {
                magnetDetector = FindFirstObjectByType<PracticeXPMagnetDetector>();
            }
        }
    }

    private void InitializeEffectHandlers()
    {
        upgradeEffectHandlers = new Dictionary<PracticeUpgradeType, Func<float, bool>>
        {
            { PracticeUpgradeType.BulletDamage, ApplyBulletDamage },
            { PracticeUpgradeType.FireRate, ApplyFireRate },
            { PracticeUpgradeType.ProjectileCount, ApplyProjectileCount },
            { PracticeUpgradeType.MaxHealth, ApplyMaxHealth },
            { PracticeUpgradeType.MoveSpeed, ApplyMoveSpeed },
            { PracticeUpgradeType.MagnetRadius, ApplyMagnetRadius }
        };
    }

    public bool ApplyUpgrade(PracticeUpgradeDefinition upgrade, int upgradeLevel)
    {
        if (upgrade == null)
        {
            return false;
        }

        if (upgradeLevel <= 0)
        {
            return false;
        }

        LoadReceivers();

        if (upgradeEffectHandlers == null)
        {
            InitializeEffectHandlers();
        }

        float value = upgrade.GetValuePerLevel(upgradeLevel);

        if (!upgradeEffectHandlers.TryGetValue(
                upgrade.UpgradeType,
                out Func<float, bool> handler
            ) || handler == null)
        {
            Debug.LogWarning(
                $"{name}: Chưa có bộ xử lý cho loại nâng cấp {upgrade.UpgradeType}.",
                this
            );
            return false;
        }

        bool appliedSuccessfully = handler.Invoke(value);

        if (appliedSuccessfully)
        {
            Debug.Log(
                $"Applied Upgrade: {upgrade.DisplayName} (Lv.{upgradeLevel}) | Value: {value}",
                this
            );
        }

        return appliedSuccessfully;
    }

    private bool ApplyBulletDamage(float value)
    {
        if (shooter == null)
        {
            return false;
        }

        int bulletDamage = Mathf.RoundToInt(value);
        if (bulletDamage <= 0)
        {
            return false;
        }

        shooter.AddBulletDamage(bulletDamage);
        return true;
    }

    private bool ApplyFireRate(float value)
    {
        if (shooter == null || value <= 0f)
        {
            return false;
        }

        shooter.ReduceShootInterval(value);
        return true;
    }

    private bool ApplyProjectileCount(float value)
    {
        if (shooter == null)
        {
            return false;
        }

        int projectileBonus = Mathf.RoundToInt(value);
        if (projectileBonus <= 0)
        {
            return false;
        }

        int previousProjectileCount = shooter.ProjectileCount;
        shooter.AddProjectileCount(projectileBonus);

        Debug.Log(
            $"{name}: Projectile Count changed from {previousProjectileCount} " +
            $"to {shooter.ProjectileCount}. Bonus = {projectileBonus}.",
            this
        );

        return true;
    }

    private bool ApplyMaxHealth(float value)
    {
        if (playerHealth == null || value <= 0f)
        {
            return false;
        }

        playerHealth.AddMaxHealth(value, true);
        return true;
    }

    private bool ApplyMoveSpeed(float value)
    {
        if (playerMovement == null || value <= 0f)
        {
            return false;
        }

        playerMovement.AddMoveSpeed(value);
        return true;
    }

    private bool ApplyMagnetRadius(float value)
    {
        if (magnetDetector == null || value <= 0f)
        {
            return false;
        }

        magnetDetector.AddMagnetRadius(value);
        return true;
    }
}
