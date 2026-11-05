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
    public event Action<float, float> HealthChanged;//hàm con ko thể nghe được do để ở hàm cha nên phải có notifyhealthchanged
    public event Action Died;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float NormalizedHealth =>
        maxHealth > 0f ? Mathf.Clamp01(currentHealth / maxHealth) : 0f;//tính tỉ lệ máu hiện tại từ 0,0 đến 1,0. Toán tử 3 ngôi bảo vệ chương trình không bao giờ bị lỗi chia cho 0
    public bool IsDead => isDead;

    protected override void Awake()
    {
        base.Awake();

        ResetHealth();
    }

    public virtual void ResetHealth()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        currentHealth = maxHealth;
        isDead = false;

        NotifyHealthChanged();//để cả lớp cha lẫn lớp con đều phát được thông báo cập nhật thanh máu UI.
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

        float previousHealth = currentHealth;

        currentHealth = Mathf.Clamp(
            currentHealth - damage,
            0f,
            maxHealth
        );

        float appliedDamage = previousHealth - currentHealth;

        if (appliedDamage <= 0f)
        {
            return;
        }

        // Health đã thật sự giảm nên mới phát event.
        DamageTaken?.Invoke(appliedDamage);
        NotifyHealthChanged();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public virtual void Heal(float healAmount)
    {
        if (isDead)
        {
            return;
        }

        if (healAmount <= 0f)
        {
            return;
        }

        float previousHealth = currentHealth;

        currentHealth = Mathf.Clamp(
            currentHealth + healAmount,
            0f,
            maxHealth
        );

        float actualHealed = currentHealth - previousHealth;

        if (actualHealed <= 0f)
        {
            return;
        }

        NotifyHealthChanged();
    }

    public virtual void AddMaxHealth(float addedMaxHealth, bool healAddedAmount = true)
    {
        if (isDead)
        {
            return;
        }

        if (addedMaxHealth <= 0f)
        {
            return;
        }

        maxHealth += addedMaxHealth;

        if (healAddedAmount)
        {
            currentHealth = Mathf.Clamp(
                currentHealth + addedMaxHealth,
                0f,
                maxHealth
            );
        }

        NotifyHealthChanged();
    }

    protected void NotifyHealthChanged()
    {
        HealthChanged?.Invoke(currentHealth, maxHealth);
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