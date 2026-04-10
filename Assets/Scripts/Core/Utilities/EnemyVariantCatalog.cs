using UnityEngine;

public static class EnemyVariantCatalog
{
    public static EnemyVariantType GetRandomVariant(float survivalTime)
    {
        EnemyVariantType[] pool = BuildVariantPool(survivalTime);

        int randomIndex = Random.Range(0, pool.Length);
        return pool[randomIndex];
    }

    public static string GetResourcePath(EnemyVariantType variantType)
    {
        switch (variantType)
        {
            case EnemyVariantType.Fast:
                return GameResourcePaths.EnemyFast;

            case EnemyVariantType.Tank:
                return GameResourcePaths.EnemyTank;

            default:
                return GameResourcePaths.EnemyGrunt;
        }
    }

    private static EnemyVariantType[] BuildVariantPool(float survivalTime)
    {
        if (survivalTime < 30f)
        {
            return new[]
            {
                EnemyVariantType.Grunt
            };
        }

        if (survivalTime < 60f)
        {
            return new[]
            {
                EnemyVariantType.Grunt,
                EnemyVariantType.Grunt,
                EnemyVariantType.Fast
            };
        }

        if (survivalTime < 90f)
        {
            return new[]
            {
                EnemyVariantType.Grunt,
                EnemyVariantType.Fast,
                EnemyVariantType.Fast,
                EnemyVariantType.Tank
            };
        }

        return new[]
        {
            EnemyVariantType.Grunt,
            EnemyVariantType.Fast,
            EnemyVariantType.Fast,
            EnemyVariantType.Tank,
            EnemyVariantType.Tank
        };
    }
}
