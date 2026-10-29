using UnityEngine;

[CreateAssetMenu(
    fileName = "BossAttackDefinition_",
    menuName = "Practice/Boss/Attack Definition"
)]
public class PracticeBossAttackDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField]
    private string attackId = "boss_attack_default";

    [SerializeField]
    private string displayName = "Boss Attack";

    [SerializeField]
    private PracticeBossAttackPatternType patternType =
        PracticeBossAttackPatternType.RadialBurst;

    [Header("Timing")]
    [Min(0f)]
    [SerializeField]
    private float initialDelay = 1f;

    [Min(0f)]
    [SerializeField]
    private float telegraphDuration = 0.75f;

    [Min(0.1f)]
    [SerializeField]
    private float attackCooldown = 2f;

    [Header("Projectile Settings")]
    [SerializeField]
    private PracticeBossProjectile projectilePrefab;

    [Min(1)]
    [SerializeField]
    private int projectileCount = 8;

    [Min(0.1f)]
    [SerializeField]
    private float projectileSpeed = 6f;

    [Min(1f)]
    [SerializeField]
    private float projectileDamage = 10f;

    [Min(0.1f)]
    [SerializeField]
    private float projectileLifetime = 5f;

    [Header("Pattern Parameters")]
    [Tooltip("Góc lệch ban đầu (dùng cho Radial Burst hoặc xoay lệch góc bắn).")]
    [SerializeField]
    private float startingAngleOffset = 0f;

    [Tooltip("Độ mở của hình quạt tính bằng độ (dùng cho Aimed Spread, ví dụ 45 độ).")]
    [Range(0f, 360f)]
    [SerializeField]
    private float spreadAngle = 45f;

    public string AttackId => attackId;
    public string DisplayName => displayName;
    public PracticeBossAttackPatternType PatternType => patternType;
    public float InitialDelay => initialDelay;
    public float TelegraphDuration => telegraphDuration;
    public float AttackCooldown => attackCooldown;
    public PracticeBossProjectile ProjectilePrefab => projectilePrefab;
    public int ProjectileCount => projectileCount;
    public float ProjectileSpeed => projectileSpeed;
    public float ProjectileDamage => projectileDamage;
    public float ProjectileLifetime => projectileLifetime;
    public float StartingAngleOffset => startingAngleOffset;
    public float SpreadAngle => spreadAngle;
}