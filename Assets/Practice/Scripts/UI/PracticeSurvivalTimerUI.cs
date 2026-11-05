using TMPro;
using UnityEngine;

public class PracticeSurvivalTimerUI : HaiMonoBehaviour
{
    [Header("References")]
    [SerializeField] protected PracticeSurvivalTimer survivalTimer;

    [Header("UI References")]
    [SerializeField] protected TextMeshProUGUI timerText;
    [SerializeField] protected TextMeshProUGUI finalTimeText;

    // Lưu đệm số giây nguyên đã hiển thị ở lần trước để chỉ nối chuỗi đúng 1 lần mỗi giây (Zero-GC)
    private int lastDisplayedSecond = -1;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadSurvivalTime();
    }

    protected void LoadSurvivalTime()
    {
        if (survivalTimer != null)
        {
            return;
        }

        survivalTimer = FindFirstObjectByType<PracticeSurvivalTimer>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        lastDisplayedSecond = -1;
        SubscribeSurvivalEvents();
    }

    protected override void OnDisable()
    {
        UnSubscribeSurvivalEvents();
        base.OnDisable();
    }

    protected override void Start()
    {
        base.Start();
        RefeshInitialUI();
    }

    protected void SubscribeSurvivalEvents()
    {
        if (survivalTimer == null) return;

        survivalTimer.TimeChanged -= HandleTimeChanged;
        survivalTimer.TimeChanged += HandleTimeChanged;

        survivalTimer.SurvivalTime -= HandleSurvivalTimer;
        survivalTimer.SurvivalTime += HandleSurvivalTimer;

        HandleTimeChanged(survivalTimer.ElapsedTime);
    }

    protected void UnSubscribeSurvivalEvents()
    {
        if (survivalTimer == null) return;

        survivalTimer.TimeChanged -= HandleTimeChanged;
        survivalTimer.SurvivalTime -= HandleSurvivalTimer;
    }

    protected void RefeshInitialUI()
    {
        lastDisplayedSecond = -1;
        float currentTime = 0f;

        if (survivalTimer != null)
        {
            currentTime = survivalTimer.ElapsedTime;
        }

        HandleTimeChanged(currentTime);

        if (finalTimeText != null)
        {
            finalTimeText.text = $"Survival Time: {FormatTime(currentTime)}";
        }
    }

    protected string FormatTime(float timeInSeconds)
    {
        float validTime = Mathf.Max(0f, timeInSeconds);
        int totalSecond = Mathf.FloorToInt(validTime);

        int minutesTime = totalSecond / 60;
        int secondTime = totalSecond % 60;
        return $"{minutesTime:00}:{secondTime:00}";
    }

    protected void HandleTimeChanged(float elapsedTime)
    {
        if (timerText == null) return;

        int currentSecond = Mathf.Max(0, Mathf.FloorToInt(elapsedTime));

        // Guard Clause Zero-GC: Nếu vẫn đang trong cùng 1 giây thì thoát ngay (0 B cấp phát chuỗi, 0 rebuild TMP Mesh)
        if (currentSecond == lastDisplayedSecond)
        {
            return;
        }

        lastDisplayedSecond = currentSecond;
        int minutesTime = currentSecond / 60;
        int secondTime = currentSecond % 60;
        timerText.text = $"{minutesTime:00}:{secondTime:00}";
    }

    protected void HandleSurvivalTimer(float finalTime)
    {
        if (finalTimeText == null) return;
        finalTimeText.text = $"Survival Time: {FormatTime(finalTime)}";
    }
}