using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerContactDamage : HaiMonoBehaviour
{
    [SerializeField] private int damageAmount = 20;
    [SerializeField] private float damageInterval = 0.5f;

    private float lastDamageTime = float.NegativeInfinity;

    protected override void ResetValues()
    {
        base.ResetValues();
        damageAmount = 20;
        damageInterval = 0.5f;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDealDamage(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDealDamage(collision);
    }

    private void TryDealDamage(Collision2D collision)
    {
        if (!CanDealDamage()) return;

        if (!TryGetEnemyHealth(collision, out EnemyHealth enemyHealth))
            return;

        ApplyDamage(enemyHealth);
    }

    private bool CanDealDamage()
    {
        return Time.time >= lastDamageTime + damageInterval;
    }

    private bool TryGetEnemyHealth(Collision2D collision, out EnemyHealth enemyHealth)
    {
        enemyHealth = collision.collider.GetComponentInParent<EnemyHealth>();
        return enemyHealth != null;
    }

    private void ApplyDamage(EnemyHealth enemyHealth)
    {
        if (enemyHealth.IsDead) return;

        enemyHealth.TakeDamage(damageAmount);
        lastDamageTime = Time.time;
    }
}