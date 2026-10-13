using TMPro;
using UnityEngine;
using System.Collections;

public class PracticeShooterStatsDebugUI : HaiMonoBehaviour
{
    [SerializeField] protected PracticeShooterWithDamage shooter;
    [SerializeField] protected TextMeshProUGUI statsText;

    protected override void LoadComponents()
    {
        base.LoadComponents();

        if (shooter == null)
        {
            shooter = GetComponent<PracticeShooterWithDamage>();
        }

        if (statsText == null)
        {
            statsText = GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    private void Update()
    {
        if (shooter == null || statsText == null) return;

        statsText.text =
            $"Damage: {shooter.BulletDamage}\n" +
            $"Fire Rate: {shooter.ShootInterval}\n" +
            $"Projectiles: {shooter.ProjectileCount}";
    }
}