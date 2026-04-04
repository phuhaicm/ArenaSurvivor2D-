using TMPro;
using UnityEngine;

public class BestRunStatsController : HaiMonoBehaviour
{
    private TextMeshProUGUI bestStatsText;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadBestStatsText();
    }

    protected override void Start()
    {
        base.Start();
        RefreshUI();
    }

    private void LoadBestStatsText()
    {
        if (bestStatsText != null) return;

        BestRunStatsDisplayTextUI marker = UIHierarchyLookup.FindInParentCanvas<BestRunStatsDisplayTextUI>(this);
        if (marker == null) return;

        bestStatsText = marker.GetComponent<TextMeshProUGUI>();
    }

    public void RefreshUI()
    {
        if (bestStatsText == null) return;

        int totalSeconds = Mathf.FloorToInt(BestRunStats.BestTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        bestStatsText.text =
            $"Best Time: {minutes:00}:{seconds:00}\n" +
            $"Best Level: {BestRunStats.BestLevel}\n" +
            $"Best XP: {BestRunStats.BestXP}";
    }
}