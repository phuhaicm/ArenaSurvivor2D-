using UnityEngine;

public static class EnemyTargetFinder
{
    public static EnemyHealth FindNearestAliveEnemy(Vector2 origin, Transform enemyContainer)
    {
        if (enemyContainer == null)
        {
            return null;
        }

        EnemyHealth nearestEnemy = null;
        float nearestDistanceSqr = float.MaxValue;

        for (int i = 0; i < enemyContainer.childCount; i++)
        {
            Transform child = enemyContainer.GetChild(i);

            if (!child.gameObject.activeInHierarchy)
            {
                continue;
            }

            EnemyHealth enemyHealth = child.GetComponent<EnemyHealth>();
            if (enemyHealth == null || enemyHealth.IsDead)
            {
                continue;
            }

            float distanceSqr = ((Vector2)child.position - origin).sqrMagnitude;

            if (distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                nearestEnemy = enemyHealth;
            }
        }

        return nearestEnemy;
    }
}