using UnityEngine;

public class PracticeShooterUpgradeApplier : HaiMonoBehaviour
{
    [Header("Reference")]
    [SerializeField] PracticeShooterWithDamage shooter;
    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadShooter();
    }
    protected void LoadShooter()
    {
        if(shooter != null) return;

        shooter = GetComponent<PracticeShooterWithDamage>();
    }
    public bool ApplyUpgrade(PracticeUpgradeDefinition upgrade, int upgradeLevel)
    {
        if(upgrade == null) return false;
        if(shooter == null) return false;
        if(upgradeLevel <= 0) return false;
        float value = upgrade.GetValuePerLevel(upgradeLevel);
        switch (upgrade.UpgradeType)
        {
            case PracticeUpgradeType.BulletDamage:
                {
                    int bulletDamage = Mathf.RoundToInt(value);//code này là chuyển từ float sang int 

                    shooter.AddBulletDamage(bulletDamage);

                    break;
                }
            case PracticeUpgradeType.FireRate:
                {
                    shooter.ReduceShootInterval(value);
                    break;
                }
            case PracticeUpgradeType.ProjectileCount:
{
    int projectileBonus =
        Mathf.RoundToInt(value);

    int previousProjectileCount =
        shooter.ProjectileCount;

    shooter.AddProjectileCount(
        projectileBonus
    );

    Debug.Log(
        $"{name}: Projectile Count changed from " +
        $"{previousProjectileCount} to " +
        $"{shooter.ProjectileCount}. " +
        $"Bonus = {projectileBonus}.",
        this
    );

    break;
}
            default:
            return false;
        }
        Debug.Log($"Applied Upgrade: " + $"{upgrade.DisplayName} | " + $"Value: {value}", this);
        return true;
    }
}
