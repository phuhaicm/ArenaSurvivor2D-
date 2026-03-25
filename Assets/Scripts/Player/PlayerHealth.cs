using UnityEngine;

public class PlayerHealth : HealthBase
{
    private PlayerMovement playerMovement;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadPlayerMovement();
    }

    protected override void ResetHealthDefaults()
    {
        maxHealth = 100;
        currentHealth = maxHealth;
        isDead = false;
    }

    protected virtual void LoadPlayerMovement()
    {
        if (playerMovement != null) return;

        playerMovement = GetComponent<PlayerMovement>();

        if (playerMovement == null)
        {
            Debug.LogWarning($"{name}: PlayerMovement not found.", gameObject);
            return;
        }

        LogLoad(nameof(LoadPlayerMovement));
    }

    protected override void OnDeath()
    {
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        Debug.Log($"{name}: Player died.", gameObject);
    }
}