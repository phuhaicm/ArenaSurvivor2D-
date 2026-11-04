public enum PracticeUpgradeCategory
{
    Offensive = 0,
    Defensive = 1,
    Utility = 2
}

public enum PracticeUpgradeType // dùng enum thay cho string để tránh lỗi gõ sai ký tự (magic string)
{
    BulletDamage = 0,
    FireRate = 1,
    ProjectileCount = 2,
    MaxHealth = 3,
    MoveSpeed = 4,
    MagnetRadius = 5,
    AuraDamage = 6,
    AuraRadius = 7
}