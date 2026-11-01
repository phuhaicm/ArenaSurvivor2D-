using UnityEngine;

public class PracticePlayerHealth : PracticeHealthBase
{
    [Header("Invulnerability Settings")]
    [SerializeField, Min(0f)]
    private float invulnerabilityDuration = 0.5f;

    private float invulnerabilityEndTime;

    public bool IsInvulnerable =>
        Time.time < invulnerabilityEndTime;

    public override void TakeDamage(float damageAmount)
    {
        if (isDead)
        {
            return;
        }

        if (damageAmount <= 0f)
        {
            return;
        }

        if (IsInvulnerable)
        {
            return;
        }

        float previousHealth = currentHealth;

        // HealthBase chịu trách nhiệm:
        // - giảm Health
        // - phát DamageTaken & HealthChanged
        // - gọi Die nếu Health bằng 0
        base.TakeDamage(damageAmount);

        float actualDamageTaken = previousHealth - currentHealth;

        if (actualDamageTaken <= 0f)
        {
            return;
        }

        // Chỉ bắt đầu bất tử nếu Player vẫn còn sống.
        if (!isDead)
        {
            invulnerabilityEndTime =
                Time.time + invulnerabilityDuration;
        }
    }

    protected override void Die()
    {
        base.Die();

        Debug.Log(
            $"{name}: Game Over.",
            this
        );
    }
}