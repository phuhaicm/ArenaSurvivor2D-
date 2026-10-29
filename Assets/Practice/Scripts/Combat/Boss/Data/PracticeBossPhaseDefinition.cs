using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "BossPhaseDefinition_",
    menuName = "Practice/Boss/Phase Definition"
)]
public class PracticeBossPhaseDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField]
    private string phaseId = "boss_phase_1";

    [SerializeField]
    private string phaseName = "Phase 1";

    [Header("Condition & Modifiers")]
    [Tooltip("Ngưỡng máu tối đa để kích hoạt Phase này (1.0 = 100% máu, 0.5 = 50% máu).")]
    [Range(0.05f, 1f)]
    [SerializeField]
    private float healthThreshold = 1f;

    [Tooltip("Hệ số nhân thời gian hồi chiêu (ví dụ 0.75 giúp Boss bắn nhanh hơn 25%).")]
    [Min(0.1f)]
    [SerializeField]
    private float cooldownMultiplier = 1f;

    [Header("Presentation")]
    [SerializeField]
    private Color phaseTintColor = Color.white;

    [SerializeField]
    private Color telegraphColor = new Color(1f, 0.25f, 0.25f, 0.45f);

    [Header("Attack Pool")]
    [SerializeField]
    private List<PracticeBossAttackDefinition> attacks =
        new List<PracticeBossAttackDefinition>();

    public string PhaseId => phaseId;
    public string PhaseName => phaseName;
    public float HealthThreshold => healthThreshold;
    public float CooldownMultiplier => Mathf.Max(0.1f, cooldownMultiplier);
    public Color PhaseTintColor => phaseTintColor;
    public Color TelegraphColor => telegraphColor;
    public IReadOnlyList<PracticeBossAttackDefinition> Attacks => attacks;

    public bool HasValidAttacks()
    {
        if (attacks == null || attacks.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < attacks.Count; i++)
        {
            if (attacks[i] != null && attacks[i].ProjectilePrefab != null)
            {
                return true;
            }
        }

        return false;
    }
}