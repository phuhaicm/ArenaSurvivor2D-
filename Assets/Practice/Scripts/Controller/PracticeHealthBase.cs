using System;
using UnityEngine;

public abstract class PracticeHealthBase
    : HaiMonoBehaviour, IPracticeDamageable
{
    [Header("Health Settings")]
    [SerializeField, Min(1f)]
    protected float maxHealth = 100f;

    [SerializeField, Min(0f)]
    protected float currentHealth;

    protected bool isDead;

    public event Action<float> DamageTaken;
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
        if (isDead)
        {
            return;
        }

        if (damage <= 0f)
        {
            return;
        }

        float previousHealth =
            currentHealth;

        currentHealth =
            Mathf.Clamp(
                currentHealth - damage,
                0f,
                maxHealth
            );

        float appliedDamage =
            previousHealth - currentHealth;

        if (appliedDamage <= 0f)
        {
            return;
        }

        // Health đã thật sự giảm nên mới phát event.
        DamageTaken?.Invoke(appliedDamage);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        Died?.Invoke();
    }
}