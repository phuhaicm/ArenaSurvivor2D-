using UnityEngine;

public class PracticeBossAimedSpreadAttackExecutor : PracticeBossAttackExecutorBase//file này tìm vị trí playermovement một lần duy nhất và bắn chùm đạn hình quạt về phía player
{
    [Header("Target Reference")]
    [SerializeField]
    private Transform playerTarget;

    public override PracticeBossAttackPatternType SupportedPatternType =>
        PracticeBossAttackPatternType.AimedSpread;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        ResolvePlayerTarget();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        ResolvePlayerTarget();
    }

    public void SetPlayerTarget(Transform targetTransform)
    {
        playerTarget = targetTransform;
    }

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

        ResolvePlayerTarget();

        float centerAngle = CalculateCenterAngleTowardsPlayer(firePoint.position) + attackDefinition.StartingAngleOffset;

        int projectileCount = Mathf.Max(1, attackDefinition.ProjectileCount);

        if (projectileCount == 1)
        {
            Vector2 singleDirection = ConvertAngleToDirection(centerAngle);
            SpawnProjectile(attackDefinition, firePoint, projectileContainer, singleDirection);
            return;
        }

        float totalSpread = Mathf.Clamp(attackDefinition.SpreadAngle, 0f, 360f);
        float startAngle = centerAngle - (totalSpread * 0.5f);
        float angleStep = totalSpread / (projectileCount - 1);

        for (int i = 0; i < projectileCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector2 direction = ConvertAngleToDirection(currentAngle);

            SpawnProjectile(
                attackDefinition,
                firePoint,
                projectileContainer,
                direction
            );
        }
    }

    private float CalculateCenterAngleTowardsPlayer(Vector3 fireOrigin)
    {
        if (playerTarget == null)
        {
            return 0f;
        }

        Vector2 toPlayer = playerTarget.position - fireOrigin;

        if (toPlayer.sqrMagnitude <= 0.0001f)
        {
            return 0f;
        }

        return Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
    }

    private void ResolvePlayerTarget()
    {
        if (playerTarget != null)
        {
            return;
        }

        PracticePlayerHealth playerHealth =
            FindFirstObjectByType<PracticePlayerHealth>();

        if (playerHealth != null)
        {
            playerTarget = playerHealth.transform;
        }
    }
}