using UnityEngine;

public class PracticeXPDropOnDeath : HaiMonoBehaviour
{
    // Lưu đệm Container ở cấp static để tránh gọi GameObject.Find("RuntimeXPOrbs") hàng trăm lần khi sinh quái
    private static Transform cachedXpOrbContainer;

    [SerializeField]
    protected PracticeEnemyHealth enemyHealth;

    [SerializeField]
    protected GameObject xpOrbPrefab;

    [SerializeField]
    protected Transform xpOrbContainer;

    protected override void LoadComponents()
    {
        base.LoadComponents();

        LoadEnemyHealth();
        LoadXPOrbContainer();
    }

    protected virtual void LoadEnemyHealth()
    {
        if (enemyHealth != null) return;

        enemyHealth = GetComponent<PracticeEnemyHealth>();
    }

    protected virtual void LoadXPOrbContainer()
    {
        if (xpOrbContainer != null) return;

        if (cachedXpOrbContainer != null)
        {
            xpOrbContainer = cachedXpOrbContainer;
            return;
        }

        GameObject containerObject = GameObject.Find("RuntimeXPOrbs");

        if (containerObject != null)
        {
            cachedXpOrbContainer = containerObject.transform;
            xpOrbContainer = cachedXpOrbContainer;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        LoadEnemyHealth();
        LoadXPOrbContainer();
        SubscribeEvent();
    }

    protected override void OnDisable()
    {
        UnsubscribeEvent();

        base.OnDisable();
    }

    protected virtual void SubscribeEvent()
    {
        if (enemyHealth == null) return;

        enemyHealth.Died -= HandleEnemyDied;
        enemyHealth.Died += HandleEnemyDied;
    }

    protected virtual void UnsubscribeEvent()
    {
        if (enemyHealth == null) return;

        enemyHealth.Died -= HandleEnemyDied;
    }

    protected virtual void HandleEnemyDied()
    {
        DropXP();
    }

    protected virtual void DropXP()
    {
        if (xpOrbPrefab == null) return;

        LoadXPOrbContainer();

        PracticeObjectPoolManager.SpawnOrInstantiate(
            xpOrbPrefab,
            transform.position,
            Quaternion.identity,
            xpOrbContainer
        );
    }
}
