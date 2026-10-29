using UnityEngine;

public class PracticeBossRadialAttackExecutor : PracticeBossAttackExecutorBase
{
    public override PracticeBossAttackPatternType SupportedPatternType =>
        PracticeBossAttackPatternType.RadialBurst;

    public override void ExecuteAttack(
        PracticeBossAttackDefinition attackDefinition,
        Transform firePoint,
        Transform projectileContainer
    )
    {
        if (attackDefinition == null || firePoint == null)
        {
            return;
        }

        int projectileCount = Mathf.Max(1, attackDefinition.ProjectileCount);
        float angleStep = 360f / projectileCount;

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = attackDefinition.StartingAngleOffset + (angleStep * i);
            Vector2 direction = ConvertAngleToDirection(angle);

            SpawnProjectile(
                attackDefinition,
                firePoint,
                projectileContainer,
                direction
            );
        }
    }
}