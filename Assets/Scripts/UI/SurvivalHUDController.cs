using TMPro;
using UnityEngine;

public class SurvivalHUDController : HaiMonoBehaviour
{
    private SurvivalTimer survivalTimer;
    private HUDRoot hudRoot;

    private TextMeshProUGUI survivalTimeText;
    private TextMeshProUGUI dangerLevelText;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadSurvivalTimer();
        LoadHUDRoot();
        LoadSurvivalTimeText();
        LoadDangerLevelText();
    }

    private void LoadSurvivalTimer()
    {
        if (survivalTimer != null) return;
        survivalTimer = FindFirstObjectByType<SurvivalTimer>();
    }

    private void LoadHUDRoot()
    {
        if (hudRoot != null) return;
        hudRoot = UIRootLookup.FindRootInCanvas<HUDRoot>(this);
    }

    private void LoadSurvivalTimeText()
    {
        if (survivalTimeText != null) return;

        SurvivalTimeTextUI marker = UIRootLookup.FindInRoot<HUDRoot, SurvivalTimeTextUI>(this);
        if (marker == null) return;

        survivalTimeText = marker.GetComponent<TextMeshProUGUI>();
    }

    private void LoadDangerLevelText()
    {
        if (dangerLevelText != null) return;

        DangerLevelTextUI marker = UIRootLookup.FindInRoot<HUDRoot, DangerLevelTextUI>(this);
        if (marker == null) return;

        dangerLevelText = marker.GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (survivalTimer == null) return;

        int totalSeconds = Mathf.FloorToInt(survivalTimer.ElapsedTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        if (survivalTimeText != null)
        {
            survivalTimeText.text = $"Time {minutes:00}:{seconds:00}";
        }

        if (dangerLevelText != null)
        {
            dangerLevelText.text = $"Danger {DifficultyDisplayUtility.GetDangerLabel(survivalTimer.ElapsedTime)}";
        }
    }
}