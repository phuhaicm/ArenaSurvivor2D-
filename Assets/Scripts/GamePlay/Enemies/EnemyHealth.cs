using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyHealth : HealthBase
{
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

    private void LoadEnemyChase()
    {
        if (enemyChase != null) return;
        enemyChase = GetComponent<EnemyChase>();
        LogLoad(nameof(LoadEnemyChase));
    }

    private void LoadEnemyContactDamage()
    {
        if (enemyContactDamage != null) return;
        enemyContactDamage = GetComponent<EnemyContactDamage>();
        LogLoad(nameof(LoadEnemyContactDamage));
    }

    private void LoadEnemyCollider()
    {
        if (enemyCollider != null) return;
        enemyCollider = GetComponent<Collider2D>();
        LogLoad(nameof(LoadEnemyCollider));
    }

    protected override void OnDeath()
    {
        if (enemyChase != null) enemyChase.enabled = false;
        if (enemyContactDamage != null) enemyContactDamage.enabled = false;
        if (enemyCollider != null) enemyCollider.enabled = false;

        if (disableObjectOnDeath)
        {
            StartCoroutine(DisableAfterDelay());
        }
    }

    private IEnumerator DisableAfterDelay()
    {
        if (disableDelay > 0f)
        {
            yield return new WaitForSeconds(disableDelay);
        }

        gameObject.SetActive(false);
    }
}