using UnityEngine;

public class PracticeEnemyContactDamage
    : HaiMonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField, Min(0f)]
    private float contactDamage = 10f;

    [SerializeField, Min(0.05f)]
    private float damageCooldown = 1f;

    private float nextDamageTime;

    private void OnTriggerStay2D(Collider2D other)
{
    if (Time.time < nextDamageTime)
    {
        return;
    }

    PracticePlayerHealth playerHealth =
        other.GetComponent<PracticePlayerHealth>();

    if (playerHealth == null)
    {
        return;
    }

    playerHealth.TakeDamage(contactDamage);

    nextDamageTime =
        Time.time + damageCooldown;
}  
}