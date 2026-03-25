using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyHealth : HealthBase
{
    [Header("Death Settings")]
    [SerializeField] private bool disableObjectOnDeath = true;
    [SerializeField] private float disableDelay = 0f;

    private EnemyChase enemyChase;
    private EnemyContactDamage enemyContactDamage;
    private Collider2D enemyCollider;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadEnemyChase();
        LoadEnemyContactDamage();
        LoadEnemyCollider();
    }

    protected override void ResetHealthDefaults()
    {
        maxHealth = 50;
        currentHealth = maxHealth;
        isDead = false;

        disableObjectOnDeath = true;
        disableDelay = 0f;
    }

    protected virtual void LoadEnemyChase()
    {
        if (enemyChase != null) return;

        enemyChase = GetComponent<EnemyChase>();
        LogLoad(nameof(LoadEnemyChase));
    }

    protected virtual void LoadEnemyContactDamage()
    {
        if (enemyContactDamage != null) return;

        enemyContactDamage = GetComponent<EnemyContactDamage>();
        LogLoad(nameof(LoadEnemyContactDamage));
    }

    protected virtual void LoadEnemyCollider()
    {
        if (enemyCollider != null) return;

        enemyCollider = GetComponent<Collider2D>();

        if (enemyCollider == null)
        {
            Debug.LogWarning($"{name}: Collider2D not found.", gameObject);
            return;
        }

        LogLoad(nameof(LoadEnemyCollider));
    }

    protected override void OnDeath()
    {
        if (enemyChase != null)
        {
            enemyChase.enabled = false;
        }

        if (enemyContactDamage != null)
        {
            enemyContactDamage.enabled = false;
        }

        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        Debug.Log($"{name}: Enemy died.", gameObject);

        if (disableObjectOnDeath)
        {
            StartCoroutine(DisableAfterDelay());
        }
    }

    protected IEnumerator DisableAfterDelay()
    {
        if (disableDelay > 0f)
        {
            yield return new WaitForSeconds(disableDelay);
        }

        gameObject.SetActive(false);
    }
}