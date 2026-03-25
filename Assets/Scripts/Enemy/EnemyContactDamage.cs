using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyContactDamage : HaiMonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] protected int damageAmount = 10;
    [SerializeField] protected float damageInterval = 1f;

    protected float lastDamageTime = float.NegativeInfinity;
    protected override void ResetValues()
    {
        base.ResetValues();

        damageAmount = 10;
        damageInterval = 1f;
    }
    protected void OnCollisionEnter2D(Collision2D collision)
    {
        TryDealDamage(collision);
    }

    protected void OnCollisionStay2D(Collision2D collision)
    {
        TryDealDamage(collision);
    }
    protected void TryDealDamage(Collision2D collision)
    {
        if (!CanDealDamage())
        {
            return;
        }

        if (!TryGetPlayerHealth(collision, out PlayerHealth playerHealth))
        {
            return;
        }

        ApplyDamage(playerHealth);
    }
    protected bool CanDealDamage()
    {
        return Time.time >= lastDamageTime + damageInterval;
    }
    protected bool TryGetPlayerHealth(Collision2D collision, out PlayerHealth playerHealth)
    {
        playerHealth = collision.collider.GetComponentInParent<PlayerHealth>();
        return playerHealth != null;
    }
    protected void ApplyDamage(PlayerHealth playerHealth)
    {
        if (playerHealth.IsDead) return;
        playerHealth.TakeDamage(damageAmount);
        lastDamageTime = Time.time;
    }
}