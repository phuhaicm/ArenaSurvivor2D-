using System.Collections.Generic;
using UnityEngine;

public static class UpgradeOptionRoller
{
    public static List<UpgradeOptionData> RollUniqueOptions(int count, float survivalTime)
    {
        List<WeightedUpgradeOption> weightedPool = PlayerUpgradeCatalog.BuildWeightedPool(survivalTime);
        List<UpgradeOptionData> results = new List<UpgradeOptionData>();

        int finalCount = Mathf.Min(count, weightedPool.Count);

        for (int i = 0; i < finalCount; i++)
        {
            int selectedIndex = GetRandomWeightedIndex(weightedPool);
            if (selectedIndex < 0) break;

            results.Add(weightedPool[selectedIndex].Option);
            weightedPool.RemoveAt(selectedIndex);
        }

        return results;
    }

    private static int GetRandomWeightedIndex(List<WeightedUpgradeOption> weightedPool)
    {
        if (weightedPool == null || weightedPool.Count == 0)
        {
            return -1;
        }

        int totalWeight = 0;

        for (int i = 0; i < weightedPool.Count; i++)
        {
            totalWeight += Mathf.Max(1, weightedPool[i].Weight);
        }

        int randomValue = Random.Range(0, totalWeight);
        int cumulative = 0;

        for (int i = 0; i < weightedPool.Count; i++)
        {
            cumulative += Mathf.Max(1, weightedPool[i].Weight);

            if (randomValue < cumulative)
            {
                return i;
            }
        }

        return weightedPool.Count - 1;
    }
}