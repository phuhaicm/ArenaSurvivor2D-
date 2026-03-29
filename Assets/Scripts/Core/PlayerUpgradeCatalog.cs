using System.Collections.Generic;
using UnityEngine;

public static class PlayerUpgradeCatalog
{
    public static List<UpgradeOptionData> GetRandomOptions(int count)
    {
        List<UpgradeOptionData> pool = CreateDefaultPool();
        Shuffle(pool);

        int finalCount = Mathf.Min(count, pool.Count);
        return pool.GetRange(0, finalCount);
    }

    private static List<UpgradeOptionData> CreateDefaultPool()
    {
        return new List<UpgradeOptionData>
        {
            new UpgradeOptionData(
                PlayerUpgradeType.MoveSpeed,
                "Swift Feet",
                "+0.75 Move Speed"
            ),
            new UpgradeOptionData(
                PlayerUpgradeType.MaxHealth,
                "Steel Body",
                "+20 Max Health"
            ),
            new UpgradeOptionData(
                PlayerUpgradeType.ContactDamage,
                "Spiked Aura",
                "+5 Contact Damage"
            )
        };
    }

    private static void Shuffle(List<UpgradeOptionData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
}