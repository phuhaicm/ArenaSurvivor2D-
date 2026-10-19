
using UnityEngine;

public class PracticeRunDirector : HaiMonoBehaviour
{
    [Header("References")]
    [SerializeField] protected PracticeSurvivalTimer survivalTimer;
    [SerializeField] protected PracticeGameStateController gameStateController;
    [Header("Run settings")]
    [SerializeField, Min(1f)] protected float runDuration = 300f;
    protected bool victoryRequest;
    public float RunDuration => runDuration;
    public float RemainingTime//cho phép đọc chứ ko được sửa hay gán giá trị tại này đang là public có nghĩa là ko được sử dụng. Đây là property chỉ đọc có phần get
    {
        get 
        {
            if(survivalTimer == null)
            {
                return runDuration;
            }
            return Mathf.Max(0f, runDuration - survivalTimer.ElapsedTime);
        }
    }
    protected override void LoadComponents()
    {
        base.LoadComponents();

        LoadSurvivalTimer();
        LoadGameStateController();
    }
    protected override void OnEnable()//đảm bảo dependency tồn tại, reset xử lý victory
    {
        base.OnEnable();

        LoadSurvivalTimer();
        LoadGameStateController();

        victoryRequest = false;
    }
    protected void Update()
    {
        CheckVictoryCondition();
    }
    protected void LoadSurvivalTimer()
    {
        if(survivalTimer != null) return;

        survivalTimer = FindFirstObjectByType<PracticeSurvivalTimer>();
    }
    protected void LoadGameStateController()
    {
        if(gameStateController != null) return;

        gameStateController = FindFirstObjectByType<PracticeGameStateController>();
    }
    protected void CheckVictoryCondition()
    {
        if(victoryRequest) return;
        if(survivalTimer == null || gameStateController == null) return;
        if(!gameStateController.IsRunning) return;
        if(survivalTimer.ElapsedTime < runDuration) return;

        RequestVictory();
    }
    protected void RequestVictory()
    {
        bool changedSuccessfully = gameStateController.RequestVictory();

        if(!changedSuccessfully && !gameStateController.IsVictory) return;
        victoryRequest = true;

        Debug.Log($"{name}: Run completed. Victory requested. ", this);
    }
}
