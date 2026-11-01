using UnityEngine;

public class PracticeEnemyContactDamage
    : HaiMonoBehaviour, IPracticePoolable
{
    [Header("Damage Settings")]
    [SerializeField, Min(0f)]
    private float contactDamage = 10f;

    [SerializeField, Min(0.05f)]
    private float damageCooldown = 1f;

    private float nextDamageTime;

    protected override void OnEnable()
    {
        base.OnEnable();
        nextDamageTime = 0f;
    }

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
            playerHealth = other.GetComponentInParent<PracticePlayerHealth>();
        }

        if (playerHealth == null || playerHealth.IsDead)
        {
            return;
        }

        playerHealth.TakeDamage(contactDamage);

        nextDamageTime =
            Time.time + damageCooldown;
    }

    public void OnTakenFromPool()
    {
        nextDamageTime = 0f;
    }

    public void OnReturnedToPool()
    {
        nextDamageTime = 0f;
    }

    public void ResetRuntimeState()
    {
        nextDamageTime = 0f;
    }
}