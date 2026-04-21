using UnityEngine;

public class PlayerUpgradeApplier : HaiMonoBehaviour
{
    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private PlayerAutoShooter playerAutoShooter;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadPlayerMovement();
        LoadPlayerHealth();
        LoadPlayerAutoShooter();
    }

    private void LoadPlayerMovement()
    {
        if (playerMovement != null) return;
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void LoadPlayerHealth()
    {
        if (playerHealth != null) return;
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void LoadPlayerAutoShooter()
    {
        if (playerAutoShooter != null) return;
        playerAutoShooter = GetComponentInChildren<PlayerAutoShooter>(true);
    }

    public void ApplyUpgrade(UpgradeOptionData option)
    {
        switch (option.Type)
        {
            case PlayerUpgradeType.MoveSpeed:
                ApplyMoveSpeedUpgrade();
                break;

            case PlayerUpgradeType.MaxHealth:
                ApplyMaxHealthUpgrade();
                break;

            case PlayerUpgradeType.BulletDamage:
                ApplyBulletDamageUpgrade();
                break;

            case PlayerUpgradeType.FireRate:
                ApplyFireRateUpgrade();
                break;

            case PlayerUpgradeType.MultiShot:
                ApplyMultiShotUpgrade();
                break;
        }

        Debug.Log($"{name}: Applied upgrade {option.DisplayName}", gameObject);
    }

    private void ApplyMoveSpeedUpgrade()
    {
        if (playerMovement != null)
        {
            playerMovement.AddMoveSpeed(0.75f);
        }
    }

    private void ApplyMaxHealthUpgrade()
    {
        if (playerHealth != null)
        {
            playerHealth.AddMaxHealth(20, true);
        }
    }

    private void ApplyBulletDamageUpgrade()
    {
        if (playerAutoShooter != null)
        {
            playerAutoShooter.AddBulletDamage(5);
        }
    }

    private void ApplyFireRateUpgrade()
    {
        if (playerAutoShooter != null)
        {
            playerAutoShooter.ReduceShootInterval(0.08f);
        }
    }

    private void ApplyMultiShotUpgrade()
    {
        if (playerAutoShooter != null)
        {
            playerAutoShooter.AddProjectileCount(1);
        }
    }
}