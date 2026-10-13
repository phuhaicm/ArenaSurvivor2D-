using System;
using UnityEngine;

public abstract class PracticeHealthBase : HaiMonoBehaviour, IPracticeDamageable
{
    [Header("Health Settings")]
    [SerializeField, Min(1f)] protected float maxHealth = 100;
    [SerializeField, Min(0f)] protected float currentHealth;
    protected bool isDead;
    public event Action Died;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;

    protected override void Awake()
    {
        base.Awake();
        currentHealth = maxHealth;
        isDead = false;
    }
    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;
        if (damage <= 0) return;

        currentHealth = Mathf.Clamp(currentHealth - damage, 0f, maxHealth);
        //Debug.Log($"{name} HP: {currentHealth}", gameObject);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }
    protected virtual void Die()
    {
        if(isDead) return;
        isDead = true;

        Died?.Invoke();
    }

}
