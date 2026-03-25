using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerHealth : HaiMonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected int currentHealth = 100;

    protected PlayerMovement playerMovement;
    public bool isDead;
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead => isDead;


    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadPlayerMovement();
    }
    protected virtual void LoadPlayerMovement()
    {
        if (playerMovement != null) return;

        playerMovement = GetComponent<PlayerMovement>();

        if (playerMovement == null)
        {
            Debug.LogWarning($"{name}: playerMovement not found.", gameObject);
            return;
        }

        
        LogLoad(nameof(playerMovement));
    }
    protected override void ResetValues()
    {
        base.ResetValues();

        maxHealth = 100;
        currentHealth = maxHealth;
        isDead = false;
    }
    protected override void Awake()
    {
        base.Awake();
        InitializeHealth();
    }
    protected void InitializeHealth()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        isDead = currentHealth <= 0;
    }
    public void TakeDamage(int damageAmount)
    {
        if(isDead) return;
        if (damageAmount <= 0) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0);


        Debug.Log($"{name}: Took {damageAmount} damage. Current Health = {currentHealth}", gameObject);

        if (currentHealth == 0)
        {
            Die();
        }
    }
    protected void Heal(int healAmount)
    {
        if (isDead) return;
        if (healAmount <= 0) return;

        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        Debug.Log($"{name}: Healed {healAmount}. Current Health = {currentHealth}", gameObject);
    }
    protected void Die()
    {
        if (isDead) return;

        isDead = true;

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        Debug.Log($"{name}: Player died.", gameObject);
    }
}
