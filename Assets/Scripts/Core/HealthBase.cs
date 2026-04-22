using System;
using UnityEngine;

public abstract class HealthBase : HaiMonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected int currentHealth = 100;

    protected bool isDead;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    
    public bool IsDead => isDead;

    public event Action Died;
    public event System.Action<int> Damaged;

    protected override void Awake()
    {
        base.Awake();
        InitializeHealth();
    }

    protected override void ResetValues()
    {
        base.ResetValues();
        ResetHealthDefaults();
    }

    protected virtual void ResetHealthDefaults()
    {
        maxHealth = 100;
        currentHealth = maxHealth;
        isDead = false;
    }

    protected virtual void InitializeHealth()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        isDead = currentHealth <= 0;
    }

    public virtual void TakeDamage(int damageAmount)
    {
        if (isDead || damageAmount <= 0) return;

        currentHealth -= damageAmount;
        Damaged?.Invoke(damageAmount);
        currentHealth = Mathf.Max(currentHealth, 0);

        OnDamaged(damageAmount);

        if (currentHealth == 0)
        {
            Die();
        }
    }

    public virtual void Heal(int healAmount)
    {
        if (isDead || healAmount <= 0) return;

        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        OnHealed(healAmount);
    }

    protected virtual void Die()
    {
        if (isDead) return;

        isDead = true;
        Died?.Invoke();
        OnDeath();
    }

    protected virtual void OnDamaged(int damageAmount) { }
    protected virtual void OnHealed(int healAmount) { }
    protected virtual void OnDeath() { }
}