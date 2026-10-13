using TMPro;
using UnityEngine;

public class PracticeSurvivalTimerUI : HaiMonoBehaviour
{
    [Header("References")]
    [SerializeField] protected PracticeSurvivalTimer survivalTimer;
    [Header("UI References")]
    [SerializeField] protected TextMeshProUGUI timerText;
    [SerializeField] protected TextMeshProUGUI finalTimeText;
    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadSurvivalTime();
    }
    protected void LoadSurvivalTime()
    {
        if(survivalTimer != null)//khác null là đã gắn rồi còn null là chưa gắn mới đi tìm gắn vào 
        {
            return;
        }
        survivalTimer = FindFirstObjectByType<PracticeSurvivalTimer>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
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
        if(survivalTimer == null) return;

        survivalTimer.TimeChanged += HandleTimeChanged;
        survivalTimer.SurvivalTime += HandleSurvivalTimer;
        //đồng bộ UI ngay với dữ liệu hiện tại
        //không phụ thuộc vào thứ tự Start giữa hai GameObject
        HandleTimeChanged(survivalTimer.ElapsedTime);
    }
    protected void UnSubscribeSurvivalEvents()
    {
        if(survivalTimer == null) return;

        survivalTimer.TimeChanged -= HandleTimeChanged;
        survivalTimer.SurvivalTime -= HandleSurvivalTimer;
    }
    protected void RefeshInitialUI()
    {
        float currentTime = 0f;

        if(survivalTimer != null)
        {
            currentTime = survivalTimer.ElapsedTime;
        }
        HandleTimeChanged(currentTime);

        if(finalTimeText != null)
        {
            finalTimeText.text = $"Survival Time: {FormatTime(currentTime)}";
        }
    }
    protected string FormatTime(float timeInSeconds)
    {
        float validTime = Mathf.Max(0, timeInSeconds);//chặn giá trị âm
        int totalSecond = Mathf.FloorToInt(timeInSeconds);

        int minutesTime = totalSecond / 60;
        int secondTime = totalSecond % 60;
        return $"{minutesTime:00}:{secondTime:00}";//ý nghĩa là yêu cầu hiển thị 2 số 0
    }
    protected void HandleTimeChanged(float elapsedTime)
    {
        if(timerText == null) return;
        timerText.text = FormatTime(elapsedTime);
    }
    protected void HandleSurvivalTimer(float finalTime)
    {
        if(finalTimeText == null) return;
        finalTimeText.text = $"Survival Time: {FormatTime(finalTime)}";
    }
}
