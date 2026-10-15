using UnityEngine;

public static class PracticeNearestEnemyFinder
{
    public static PracticeEnemyHealth FindNearest(Vector2 origin) 
    {
        PracticeEnemyHealth[] enemies = Object.FindObjectsOfType<PracticeEnemyHealth>();

        PracticeEnemyHealth nearest = null;
        float nearestDistance = float.MaxValue;
        foreach (PracticeEnemyHealth enemy in enemies)
        {
            if (enemy == null) continue;
            // tiếp tục là bởi khi nếu enemy này null thì tiếp tục enemy tiếp theo
            float distance = Vector2.Distance(origin, enemy.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = enemy;
            }
        }
        return nearest;

    }
}

