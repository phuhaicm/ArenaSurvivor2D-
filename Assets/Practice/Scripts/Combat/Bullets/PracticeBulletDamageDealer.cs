using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PracticeBulletDamageDealer : HaiMonoBehaviour, IPracticePoolable
{
    [SerializeField]
    protected int damageAmount = 10;

    [SerializeField]
    protected Collider2D bulletCollider;

    private bool hasDealtDamage;//chặn lỗi 1 viên đạn xuyên 2 con quái

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadBulletCollider();
    }

    protected override void Awake()
    {
        base.Awake();
        LoadBulletCollider();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        LoadBulletCollider();
        ResetRuntimeState();
    }

    protected override void ResetValues()
    {
        base.ResetValues();
        damageAmount = 10;
    }

    private void LoadBulletCollider()
    {
        if (bulletCollider != null)
        {
            return;
        }

        bulletCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasDealtDamage || other == null)
        {
            return;
        }

        PracticeEnemyHealth enemyHealth =
            other.GetComponentInParent<PracticeEnemyHealth>();

        if (enemyHealth == null || enemyHealth.IsDead)
        {
            return;
        }

        hasDealtDamage = true;

        if (bulletCollider != null)
        {
            bulletCollider.enabled = false;
            //phải có cái này và hasDealtDamage bởi vì nếu 2 con quái đứng sát nhau khi viên đạn chạm vào ranh giới
            // sẽ kích hoạt 2 lần liên tiếp sinh ra đạn ma
        }

        enemyHealth.TakeDamage(damageAmount);
        PracticeObjectPoolManager.DespawnOrDestroy(gameObject);
    }

    public void SetDamage(int amount)
    {
        damageAmount = Mathf.Max(1, amount);
    }

    public void OnTakenFromPool()
    {
        hasDealtDamage = false;

        if (bulletCollider != null)
        {
            bulletCollider.enabled = true;
        }
    }

    public void OnReturnedToPool()
    {
        hasDealtDamage = false;

        if (bulletCollider != null)
        {
            bulletCollider.enabled = false;
        }
    }

    public void ResetRuntimeState()
    {
        hasDealtDamage = false;

        if (bulletCollider != null)
        {
            bulletCollider.enabled = true;
        }
    }
}