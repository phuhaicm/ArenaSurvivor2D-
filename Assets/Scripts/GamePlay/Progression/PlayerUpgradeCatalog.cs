using System.Collections.Generic;

public static class PlayerUpgradeCatalog
{
    public static List<WeightedUpgradeOption> BuildWeightedPool(float survivalTime)
    {
        return new List<WeightedUpgradeOption>
        {
            new WeightedUpgradeOption(
                new UpgradeOptionData(
                    PlayerUpgradeType.MoveSpeed,
                    "Swift Feet",
                    "+0.75 Move Speed"
                ),
                GetMoveSpeedWeight(survivalTime)
            ),

            new WeightedUpgradeOption(
                new UpgradeOptionData(
                    PlayerUpgradeType.MaxHealth,
                    "Steel Body",
                    "+20 Max Health"
                ),
                GetMaxHealthWeight(survivalTime)
            ),

            new WeightedUpgradeOption(
                new UpgradeOptionData(
                    PlayerUpgradeType.BulletDamage,
                    "Sharpened Rounds",
                    "+5 Bullet Damage"
                ),
                GetBulletDamageWeight(survivalTime)
            ),

            new WeightedUpgradeOption(
                new UpgradeOptionData(
                    PlayerUpgradeType.FireRate,
                    "Rapid Trigger",
                    "-0.08 Shoot Interval"
                ),
                GetFireRateWeight(survivalTime)
            ),

            new WeightedUpgradeOption(
                new UpgradeOptionData(
                    PlayerUpgradeType.MultiShot,
                    "Twin Burst",
                    "+1 Projectile"
                ),
                GetMultiShotWeight(survivalTime)
            ),

            new WeightedUpgradeOption(
                new UpgradeOptionData(
                    PlayerUpgradeType.MagnetRadius,
                    "Magnet Core",
                    "+0.75 Pickup Radius"
                ),
                GetMagnetRadiusWeight(survivalTime)
            )
        };
    }

    private static int GetMoveSpeedWeight(float survivalTime)
    {
        if (survivalTime < 60f) return 3;
        if (survivalTime < 120f) return 2;
        return 1;
    }

    private static int GetMaxHealthWeight(float survivalTime)
    {
        if (survivalTime < 60f) return 3;
        if (survivalTime < 120f) return 2;
        return 2;
    }

    private static int GetBulletDamageWeight(float survivalTime)
    {
        return 3;
    }

    private static int GetFireRateWeight(float survivalTime)
    {
        if (survivalTime < 60f) return 2;
        if (survivalTime < 120f) return 3;
        return 3;
    }

    private static int GetMultiShotWeight(float survivalTime)
    {
        if (survivalTime < 60f) return 1;
        if (survivalTime < 120f) return 2;
        return 3;
    }

    private static int GetMagnetRadiusWeight(float survivalTime)
    {
        if (survivalTime < 60f) return 2;
        if (survivalTime < 120f) return 2;
        return 3;
    }
}
