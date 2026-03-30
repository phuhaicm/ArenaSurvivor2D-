using UnityEngine;

public class XPDropOnDeath : HaiMonoBehaviour
{
    [SerializeField] private int xpValue = 10;

    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private DropContainerRoot dropContainerRoot;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadEnemyHealth();
        LoadDropContainerRoot();
    }

    protected override void ResetValues()
    {
        base.ResetValues();
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

    private void LoadDropContainerRoot()
    {
        if (dropContainerRoot != null) return;
        dropContainerRoot = FindFirstObjectByType<DropContainerRoot>();
    }

    private void SpawnXPDrop()
    {
        ExperienceOrbSize size = ExperienceOrbCatalog.GetSizeFromValue(xpValue);
        ExperienceOrbConfig config = ExperienceOrbCatalog.GetConfig(size);

        XPPickup prefab = ResourcePrefabLoader.LoadPrefab<XPPickup>(config.ResourcePath);
        if (prefab == null) return;

        Transform parent = dropContainerRoot != null ? dropContainerRoot.transform : null;
        XPPickup drop = Instantiate(prefab, transform.position, Quaternion.identity, parent);
        drop.SetExperienceValue(config.ExperienceValue);
    }
}
