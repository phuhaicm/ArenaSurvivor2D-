using UnityEngine;

public abstract class PracticeBossAttackExecutorBase : HaiMonoBehaviour//file này dùng chung để sinh viên đạn và chuyển đổi góc sang vector có hướng, còn góc bắn cụ thể do các lớp con đảm nhận
{
    public abstract PracticeBossAttackPatternType SupportedPatternType { get; }

    public abstract void ExecuteAttack(
        PracticeBossAttackDefinition attackDefinition,
        Transform firePoint,
        Transform projectileContainer
    );

    protected void SpawnProjectile(
        PracticeBossAttackDefinition attack,
        Transform firePoint,
        Transform projectileContainer,
        Vector2 direction
    )
    {
        if (attack == null || attack.ProjectilePrefab == null || firePoint == null)
        {
            return;
        }

        PracticeBossProjectile projectile = Instantiate(
            attack.ProjectilePrefab,
            firePoint.position,
            Quaternion.identity,
            projectileContainer
        );

        if (projectile == null)
        {
            return;
        }

        projectile.Initialize(
            direction,
            attack.ProjectileSpeed,
            attack.ProjectileDamage,
            attack.ProjectileLifetime
        );
    }

    protected Vector2 ConvertAngleToDirection(float angleInDegrees)
    {
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;

        return new Vector2(
            Mathf.Cos(angleInRadians),
            Mathf.Sin(angleInRadians)
        ).normalized;
    }
}