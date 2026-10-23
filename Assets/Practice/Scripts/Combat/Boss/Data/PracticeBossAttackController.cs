using System.Collections;
using UnityEngine;

public class PracticeBossAttackController
    : HaiMonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PracticeEnemyHealth bossHealth;

    [SerializeField]
    private PracticeGameStateController gameStateController;

    [SerializeField]
    private Transform firePoint;

    [SerializeField]
    private SpriteRenderer telegraphRenderer;//đây là visual cảnh báo trước attack

    [SerializeField]
    private Transform projectileContainer;

    [Header("Attack Data")]
    [SerializeField]
    private PracticeBossAttackDefinition attackDefinition;

    private Coroutine attackLoopCoroutine;
    private bool attackInProgress;

    protected override void LoadComponents()
    {
        base.LoadComponents();

        LoadBossHealth();
        LoadGameStateController();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        LoadBossHealth();
        LoadGameStateController();

        SubscribeBossEvents();
        ResetAttackPresentation();
        StartAttackLoop();
    }

    protected override void OnDisable()
    {
        UnsubscribeBossEvents();
        StopAttackLoop();
        ResetAttackPresentation();

        base.OnDisable();
    }

    private void LoadBossHealth()
    {
        if (bossHealth != null)
        {
            return;
        }

        bossHealth =
            GetComponent<PracticeEnemyHealth>();
    }

    private void LoadGameStateController()
    {
        if (gameStateController != null)
        {
            return;
        }

        gameStateController =
            FindFirstObjectByType<
                PracticeGameStateController
            >();
    }

    private void SubscribeBossEvents()
    {
        if (bossHealth == null)
        {
            return;
        }

        bossHealth.Died -=
            HandleBossDied;

        bossHealth.Died +=
            HandleBossDied;
    }

    private void UnsubscribeBossEvents()
    {
        if (bossHealth == null)
        {
            return;
        }

        bossHealth.Died -=
            HandleBossDied;
    }

    private void StartAttackLoop()
    {
        if (!CanStartAttackLoop())
        {
            return;
        }

        StopAttackLoop();

        attackLoopCoroutine =
            StartCoroutine(
                AttackLoopRoutine()
            );
    }

    private bool CanStartAttackLoop()
    {
        if (attackDefinition == null)
        {
            Debug.LogError(
                $"{name}: Attack Definition đang null.",
                this
            );

            return false;
        }

        if (attackDefinition.ProjectilePrefab == null)
        {
            Debug.LogError(
                $"{name}: Projectile Prefab đang null.",
                this
            );

            return false;
        }

        if (firePoint == null)
        {
            Debug.LogError(
                $"{name}: Fire Point đang null.",
                this
            );

            return false;
        }

        if (bossHealth == null ||
            bossHealth.IsDead)
        {
            return false;
        }

        return true;
    }

    private IEnumerator AttackLoopRoutine()
    {
        if (attackDefinition.InitialDelay > 0f)
        {
            yield return WaitUntilRunningForSeconds(
                attackDefinition.InitialDelay//boss ngay khi spawn ko bắn ngay lập tức
            );
        }

        while (CanContinueAttacking())
        {
            yield return WaitUntilGameIsRunning();

            if (!CanContinueAttacking())
            {
                break;
            }

            yield return PerformAttackRoutine();

            if (!CanContinueAttacking())
            {
                break;
            }

            yield return WaitUntilRunningForSeconds(
                attackDefinition.AttackCooldown
            );
        }

        attackInProgress = false;
        attackLoopCoroutine = null;
    }

    private IEnumerator PerformAttackRoutine()//1 lần attack hoàn chỉnh
    {
        attackInProgress = true;

        SetTelegraphActive(true);

        if (attackDefinition.TelegraphDuration > 0f)
        {
            yield return WaitUntilRunningForSeconds(
                attackDefinition.TelegraphDuration
            );
        }

        SetTelegraphActive(false);

        if (!CanContinueAttacking())
        {
            attackInProgress = false;
            yield break;
        }

        FireRadialBurst();

        attackInProgress = false;
    }

    private void FireRadialBurst()
    {
        int projectileCount =
            Mathf.Max(
                1,
                attackDefinition.ProjectileCount
            );

        float angleStep =
            360f / projectileCount;

        for (int i = 0;
             i < projectileCount;
             i++)
        {
            float angle =
                attackDefinition.StartingAngleOffset +
                angleStep * i;

            Vector2 direction =
                ConvertAngleToDirection(angle);//chuyển góc thành vector hướng

            SpawnProjectile(direction);
        }
    }

    private Vector2 ConvertAngleToDirection(
        float angleInDegrees
    )
    {
        float angleInRadians =
            angleInDegrees *
            Mathf.Deg2Rad;

        return new Vector2(
            Mathf.Cos(angleInRadians),
            Mathf.Sin(angleInRadians)
        ).normalized;
    }

    private void SpawnProjectile(
        Vector2 direction
    )
    {
        PracticeBossProjectile projectile =
            Instantiate(
                attackDefinition.ProjectilePrefab,
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
            attackDefinition.ProjectileSpeed,
            attackDefinition.ProjectileDamage,
            attackDefinition.ProjectileLifetime
        );
    }

    private IEnumerator WaitUntilGameIsRunning()
    {
        while (gameStateController != null &&
               !gameStateController.IsRunning)
        {
            if (!CanContinueAttacking())
            {
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator WaitUntilRunningForSeconds(
        float duration
    )
    {
        if (duration <= 0f)
        {
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (!CanContinueAttacking())
            {
                yield break;
            }

            if (gameStateController != null &&
                gameStateController.IsRunning)
            {
                elapsedTime +=
                    Time.deltaTime;//chỉ cộng thời gian khi game running
            }

            yield return null;
        }
    }

    private bool CanContinueAttacking()
    {
        if (!isActiveAndEnabled)
        {
            return false;
        }

        if (bossHealth == null ||
            bossHealth.IsDead)
        {
            return false;
        }

        if (gameStateController == null)
        {
            return false;
        }

        if (gameStateController.IsGameOver ||
            gameStateController.IsVictory)
        {
            return false;
        }

        return true;
    }

    private void HandleBossDied()
    {
        StopAttackLoop();
        ResetAttackPresentation();
    }

    private void StopAttackLoop()
    {
        if (attackLoopCoroutine == null)
        {
            return;
        }

        StopCoroutine(
            attackLoopCoroutine
        );

        attackLoopCoroutine = null;
        attackInProgress = false;
    }

    private void ResetAttackPresentation()
    {
        SetTelegraphActive(false);
    }

    private void SetTelegraphActive(
        bool isActive
    )
    {
        if (telegraphRenderer == null)
        {
            return;
        }

        telegraphRenderer.enabled =
            isActive;
    }
}