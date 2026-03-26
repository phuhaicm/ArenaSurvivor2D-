using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyContactDamage : ContactDamageDealer
{
    protected override void ResetDamageDefaults()
    {
        damageAmount = 10;
        damageInterval = 1f;
    }

    protected override bool TryGetDamageable(Collision2D collision, out IDamageable damageable)
    {
        damageable = collision.collider.GetComponentInParent<PlayerHealth>();
        return damageable != null;
    }
}