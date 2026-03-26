using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerContactDamage : ContactDamageDealer
{
    protected override void ResetDamageDefaults()
    {
        damageAmount = 20;
        damageInterval = 0.5f;
    }

    protected override bool TryGetDamageable(Collision2D collision, out IDamageable damageable)
    {
        damageable = collision.collider.GetComponentInParent<EnemyHealth>();
        return damageable != null;
    }
}