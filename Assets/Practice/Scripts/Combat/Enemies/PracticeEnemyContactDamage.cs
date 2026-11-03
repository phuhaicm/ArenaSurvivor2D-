using UnityEngine;

public class PracticeEnemyContactDamage
    : HaiMonoBehaviour, IPracticePoolable
{
    private const int PlayerBodyLayer = 3;

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
        if (other == null)
        {
            return;
        }

        // CHỐT CHẶN O(1) QUAN TRỌNG NHẤT:
        // Nếu vật chạm vào không nằm ở Layer PlayerBody (3) và không mang Tag "Player",
        // thoát ngay lập tức (0 gọi GetComponent, 0 tốn CPU khi 1.000 con quái đứng chồng lên nhau!)
        GameObject otherObj = other.gameObject;
        if (otherObj.layer != PlayerBodyLayer && !otherObj.CompareTag("Player"))
        {
            return;
        }

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
        nextDamageTime = Time.time + damageCooldown;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision == null || collision.collider == null)
        {
            return;
        }

        OnTriggerStay2D(collision.collider);
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
