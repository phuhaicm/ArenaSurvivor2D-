using TMPro;
using UnityEngine;

public class PracticeShooterStatsDebugUI : HaiMonoBehaviour
{
    [SerializeField]
    protected PracticeShooterWithDamage shooter;

    [SerializeField]
    protected TextMeshProUGUI statsText;

    // Lưu đệm các giá trị đã hiển thị ở lần trước để tránh nối chuỗi (GC Alloc) mỗi frame trong Update
    private int cachedBulletDamage = -1;
    private float cachedShootInterval = -1f;
    private int cachedProjectileCount = -1;

    protected override void LoadComponents()
    {
        base.LoadComponents();

        if (shooter == null)
        {
            shooter = GetComponent<PracticeShooterWithDamage>();
            if (shooter == null)
            {
                shooter = FindFirstObjectByType<PracticeShooterWithDamage>();
            }
        }

        if (statsText == null)
        {
            statsText = GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    protected override void Awake()
    {
        base.Awake();

#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        // Tự động ẩn Debug UI trong bản Release Build chính thức (Mục 5.23 PROJECT_CONTEXT.md)
        gameObject.SetActive(false);
        return;
#endif
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        InvalidateCache();
        RefreshStatsTextIfNeeded();
    }

    private void Update()
    {
        RefreshStatsTextIfNeeded();
    }

    private void InvalidateCache()
    {
        cachedBulletDamage = -1;
        cachedShootInterval = -1f;
        cachedProjectileCount = -1;
    }

    private void RefreshStatsTextIfNeeded()
    {
        if (shooter == null || statsText == null)
        {
            return;
        }

        int currentDamage = shooter.BulletDamage;
        float currentInterval = shooter.ShootInterval;
        int currentProjectiles = shooter.ProjectileCount;

        // Guard Clause: Nếu cả 3 chỉ số súng không hề thay đổi so với frame trước -> Thoát ngay (0 GC Alloc!)
        if (currentDamage == cachedBulletDamage &&
            Mathf.Approximately(currentInterval, cachedShootInterval) &&
            currentProjectiles == cachedProjectileCount)
        {
            return;
        }

        cachedBulletDamage = currentDamage;
        cachedShootInterval = currentInterval;
        cachedProjectileCount = currentProjectiles;

        statsText.text =
            $"Damage: {cachedBulletDamage}\n" +
            $"Fire Rate: {cachedShootInterval:F2}\n" +
            $"Projectiles: {cachedProjectileCount}";
    }
}