using UnityEngine;

public class PlayerUpgradeApplier : HaiMonoBehaviour
{
    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private PlayerContactDamage playerContactDamage;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadPlayerMovement();
        LoadPlayerHealth();
        LoadPlayerContactDamage();
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

    private void LoadPlayerContactDamage()
    {
        if (playerContactDamage != null) return;
        playerContactDamage = GetComponent<PlayerContactDamage>();
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

            case PlayerUpgradeType.ContactDamage:
                ApplyContactDamageUpgrade();
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

    private void ApplyContactDamageUpgrade()
    {
        if (playerContactDamage != null)
        {
            playerContactDamage.AddDamage(5);
        }
    }
}