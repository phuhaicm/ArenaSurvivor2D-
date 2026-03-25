using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyHealth : HaiMonoBehaviour
{
    [Header("Chase Settings")]
    [SerializeField] protected int maxHealth = 50;
    [SerializeField] protected int currentHealth = 50;

    protected EnemyChase enemyChase;
    protected PlayerContactDamage contactDamage;

    protected bool isDead;
    public bool IsDead => isDead;
    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadEnemyChase();
        LoadEnemyContactDamage();
    }

    protected override void ResetValues()
    {
        base.ResetValues();
        maxHealth = 50;
        currentHealth = maxHealth;
        isDead = false;
    }

    protected virtual void LoadEnemyChase()
    {
        if (enemyChase != null) return;

        enemyChase = GetComponent<EnemyChase>();
        LogLoad(nameof(LoadEnemyChase));
    }
    protected virtual void LoadEnemyContactDamage()
    {
        if (contactDamage != null) return;

        contactDamage = GetComponent<PlayerContactDamage>();
        LogLoad(nameof(LoadEnemyContactDamage));
    }
    protected override void Awake()
    {
        base.Awake();
        InitializeHealth();
    }
    protected void InitializeHealth()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        isDead = currentHealth <= 0;
    }
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;
        if (damageAmount <= 0) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0);


        Debug.Log($"{name}: Took {damageAmount} damage", gameObject);

        if (currentHealth == 0)
        {
            Die();
        }
    }
    protected void Die()
    {
        if (isDead) return;

        isDead = true;

        if (enemyChase != null)
        {
            enemyChase.enabled = false;
        }
        if (contactDamage != null)
        {
            contactDamage.enabled = false;
        }

        Debug.Log($"{name}: Enemies died.", gameObject);
    }





}