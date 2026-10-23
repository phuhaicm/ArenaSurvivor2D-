using UnityEngine;

[CreateAssetMenu(
    fileName = "BossAttackDefinition_",
    menuName = "Practice/Boss/Boss Attack Definition"
)]
public class PracticeBossAttackDefinition
    : ScriptableObject
{
    [Header("Identity")]
    [SerializeField]
    private string attackId =
        "boss_radial_burst";

    [SerializeField]
    private string displayName =
        "Radial Burst";

    [Header("Timing")]
    [SerializeField, Min(0f)]
    private float initialDelay = 1.5f;

    [SerializeField, Min(0f)]
    private float telegraphDuration = 0.8f;

    [SerializeField, Min(0.1f)]
    private float attackCooldown = 2.5f;

    [Header("Projectile")]
    [SerializeField]
    private PracticeBossProjectile projectilePrefab;

    [SerializeField, Min(1)]
    private int projectileCount = 12;

    [SerializeField, Min(0.1f)]
    private float projectileSpeed = 4f;

    [SerializeField, Min(1f)]
    private float projectileDamage = 10f;

    [SerializeField, Min(0.1f)]
    private float projectileLifetime = 5f;

    [Header("Pattern")]
    [SerializeField]
    private float startingAngleOffset;

    public string AttackId =>
        attackId;

    public string DisplayName =>
        displayName;

    public float InitialDelay =>
        initialDelay;

    public float TelegraphDuration =>
        telegraphDuration;

    public float AttackCooldown =>
        attackCooldown;

    public PracticeBossProjectile ProjectilePrefab =>
        projectilePrefab;

    public int ProjectileCount =>
        projectileCount;

    public float ProjectileSpeed =>
        projectileSpeed;

    public float ProjectileDamage =>
        projectileDamage;

    public float ProjectileLifetime =>
        projectileLifetime;

    public float StartingAngleOffset =>
        startingAngleOffset;
}