using UnityEngine;

public class XPDropOnDeath : HaiMonoBehaviour
{
    [SerializeField] private string xpOrbPrefabPath = GameResourcePaths.XPOrb;
    [SerializeField] private int xpValue = 10;

    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private XPPickup xpOrbPrefab;
    [SerializeField] private DropContainerRoot dropContainerRoot;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadEnemyHealth();
        LoadXPPrefab();
        LoadDropContainerRoot();
    }

    protected override void ResetValues()
    {
        base.ResetValues();
        xpOrbPrefabPath = GameResourcePaths.XPOrb;
        xpValue = 10;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (enemyHealth != null)
        {
            enemyHealth.Died += SpawnXPDrop;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (enemyHealth != null)
        {
            enemyHealth.Died -= SpawnXPDrop;
        }
    }

    private void LoadEnemyHealth()
    {
        if (enemyHealth != null) return;
        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void LoadXPPrefab()
    {
        if (xpOrbPrefab != null) return;
        xpOrbPrefab = ResourcePrefabLoader.LoadPrefab<XPPickup>(xpOrbPrefabPath);
    }

    private void LoadDropContainerRoot()
    {
        if (dropContainerRoot != null) return;
        dropContainerRoot = FindFirstObjectByType<DropContainerRoot>();
    }

    private void SpawnXPDrop()
    {
        if (xpOrbPrefab == null) return;

        Transform parent = dropContainerRoot != null ? dropContainerRoot.transform : null;
        XPPickup drop = Instantiate(xpOrbPrefab, transform.position, Quaternion.identity, parent);
        drop.SetExperienceValue(xpValue);
    }
}