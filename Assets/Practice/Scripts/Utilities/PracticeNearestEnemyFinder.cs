using System.Collections.Generic;
using UnityEngine;

public static class PracticeNearestEnemyFinder
{
    private const float DefaultRetargetInterval = 0.08f;

    private static PracticeEnemyHealth cachedNearestTarget;
    private static float nextSearchTime;

    // Giữ nguyên chính xác chữ ký hàm gốc FindNearest(Vector2 origin) để tương thích 100% với PracticeShooterWithDamage
    public static PracticeEnemyHealth FindNearest(Vector2 origin)
    {
        return FindNearest(origin, float.MaxValue, true);
    }

    public static PracticeEnemyHealth FindNearest(
        Vector2 origin,
        float maxRange,
        bool prioritizeBoss = true
    )
    {
        // Chốt chặn O(1): Nếu trên bản đồ không có con quái nào đang sống, thoát ngay lập tức (0 tốn CPU, 0 GC)
        if (PracticeTargetRegistry.Count == 0)
        {
            cachedNearestTarget = null;
            return null;
        }

        // Nếu mục tiêu đang khóa vẫn còn sống trong tầm bắn và chưa tới chu kỳ quét lại -> Dùng lại ngay (O(1))
        if (Time.time < nextSearchTime &&
            IsTargetValid(cachedNearestTarget, origin, maxRange))
        {
            return cachedNearestTarget;
        }

        nextSearchTime = Time.time + DefaultRetargetInterval;
        cachedNearestTarget = QueryNearestFromRegistry(origin, maxRange, prioritizeBoss);

        return cachedNearestTarget;
    }

    private static PracticeEnemyHealth QueryNearestFromRegistry(
        Vector2 origin,
        float maxRange,
        bool prioritizeBoss
    )
    {
        IReadOnlyList<PracticeEnemyHealth> targets = PracticeTargetRegistry.ActiveTargets;
        int count = targets.Count;

        if (count == 0)
        {
            return null;
        }

        float maxRangeSqr = maxRange >= float.MaxValue * 0.5f
            ? float.MaxValue
            : maxRange * maxRange;

        PracticeEnemyHealth nearestNormal = null;
        float nearestNormalDistSqr = maxRangeSqr;

        PracticeEnemyHealth nearestBoss = null;
        float nearestBossDistSqr = maxRangeSqr;

        // Duyệt bằng vòng lặp for ngược để đạt 0 GC Alloc và tự làm sạch nếu có phần tử không hợp lệ
        for (int i = count - 1; i >= 0; i--)
        {
            PracticeEnemyHealth enemy = targets[i];

            if (enemy == null || enemy.IsDead || !enemy.gameObject.activeInHierarchy)
            {
                PracticeTargetRegistry.UnregisterTarget(enemy);
                continue;
            }

            Vector2 offset = (Vector2)enemy.transform.position - origin;

            // Dùng sqrMagnitude (x^2 + y^2) thay vì Vector2.Distance để loại bỏ hoàn toàn phép căn bậc hai (Mathf.Sqrt)
            float distanceSqr = offset.sqrMagnitude;

            if (prioritizeBoss && enemy.IsBossTarget)
            {
                if (distanceSqr < nearestBossDistSqr)
                {
                    nearestBossDistSqr = distanceSqr;
                    nearestBoss = enemy;
                }
            }

            if (distanceSqr < nearestNormalDistSqr)
            {
                nearestNormalDistSqr = distanceSqr;
                nearestNormal = enemy;
            }
        }

        if (prioritizeBoss && nearestBoss != null)
        {
            return nearestBoss;
        }

        return nearestNormal;
    }

    private static bool IsTargetValid(
        PracticeEnemyHealth target,
        Vector2 origin,
        float maxRange
    )
    {
        if (target == null || target.IsDead || !target.gameObject.activeInHierarchy)
        {
            return false;
        }

        if (maxRange >= float.MaxValue * 0.5f)
        {
            return true;
        }

        Vector2 offset = (Vector2)target.transform.position - origin;
        return offset.sqrMagnitude <= (maxRange * maxRange);
    }
}