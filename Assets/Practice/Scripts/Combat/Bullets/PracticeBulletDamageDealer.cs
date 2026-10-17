
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PracticeBulletDamageDealer : HaiMonoBehaviour
{
    [SerializeField] protected int damageAmount = 10;

    protected override void ResetValues()
    {
        base.ResetValues();
        damageAmount = 10;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PracticeEnemyHealth enemyHealth = other.GetComponentInParent<PracticeEnemyHealth>();

        if(enemyHealth == null) return;
    
        enemyHealth.TakeDamage(damageAmount);
        Destroy(gameObject);
    }

    public void SetDamage(int amount)
    {
        damageAmount = Mathf.Max(1, amount);
    }

}
