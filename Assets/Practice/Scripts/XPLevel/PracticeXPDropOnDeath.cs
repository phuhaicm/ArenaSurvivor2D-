
using UnityEngine;

public class PracticeXPDropOnDeath : HaiMonoBehaviour
{
    [SerializeField] protected PracticeEnemyHealth enemyHealth;
    [SerializeField] protected GameObject xpOrbPrefab;

    protected override void LoadComponents()
    {
        base.LoadComponents();

        LoadEnemyHealth();
    }

    protected virtual void LoadEnemyHealth()
    {
        if(enemyHealth != null) return;

        enemyHealth = GetComponent<PracticeEnemyHealth>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        SubscribeEvent();
    } 
    protected override void OnDisable()
    {
        base.OnDisable();

        UnsubscribeEvent();
    }
    protected virtual void SubscribeEvent()
    {
        if(enemyHealth == null) return;

        enemyHealth.Died += HandleEnemyDied;
    }
    
    protected virtual void UnsubscribeEvent()
    {
        if(enemyHealth == null) return;

        enemyHealth.Died -= HandleEnemyDied;
    }

    protected virtual void HandleEnemyDied()
    {
        // Implementation for handling enemy death
        DropXP();
    }

    protected virtual void DropXP()
    {
        if (xpOrbPrefab == null) return;
        
        Instantiate(xpOrbPrefab, transform.position, Quaternion.identity);
        
    }
}
