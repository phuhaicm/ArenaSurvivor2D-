using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerBulletDamageDealer : HaiMonoBehaviour
{
    [SerializeField] private int damageAmount = 20;

    protected override void ResetValues()
    {
        base.ResetValues();
        damageAmount = 20;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
        if (enemyHealth == null || enemyHealth.IsDead)
        {
            return;
        }

        enemyHealth.TakeDamage(damageAmount);
        Destroy(gameObject);
    }

    public void SetDamage(int amount)
    {
        damageAmount = Mathf.Max(1, amount);
    }
}