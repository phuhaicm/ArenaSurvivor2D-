using UnityEngine;

public abstract class ContactDamageDealer : HaiMonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] protected int damageAmount = 10;
    [SerializeField] protected float damageInterval = 1f;

    protected float lastDamageTime = float.NegativeInfinity;

    protected override void ResetValues()
    {
        base.ResetValues();
        ResetDamageDefaults();
    }

    protected virtual void ResetDamageDefaults()
    {
        damageAmount = 10;
        damageInterval = 1f;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDealDamage(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDealDamage(collision);
    }

    protected virtual void TryDealDamage(Collision2D collision)
    {
        if (!CanDealDamage()) return;
        if (!TryGetDamageable(collision, out IDamageable damageable)) return;
        if (damageable.IsDead) return;

        damageable.TakeDamage(damageAmount);
        lastDamageTime = Time.time;
        OnDamageDealt(damageable);
    }

    protected virtual bool CanDealDamage()
    {
        return Time.time >= lastDamageTime + damageInterval;
    }

    protected abstract bool TryGetDamageable(Collision2D collision, out IDamageable damageable);

    protected virtual void OnDamageDealt(IDamageable damageable) { }
}
