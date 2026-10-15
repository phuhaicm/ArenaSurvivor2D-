using UnityEngine;
using System;
public class PracticeSurvivalTimer : HaiMonoBehaviour
{
    [Header("References")]
    [SerializeField] private PracticePlayerHealth playerHealth;
    [SerializeField] protected float elapsedTime;
    [SerializeField] protected bool isRunning;
    public event Action<float> TimeChanged; 
    public event Action<float> SurvivalTime; 
    public float ElapsedTime => elapsedTime;
    public bool Isrunning => isRunning;
    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadPlayerHealth();
    }
    protected void LoadPlayerHealth()
    {
        if(playerHealth != null)//khác null là đã gắn rồi còn null là chưa gắn mới đi tìm gắn vào 
        {
            return;
        }
        playerHealth = FindFirstObjectByType<PracticePlayerHealth>();
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        SubscribePlayerEvents();
    }
    protected override void OnDisable()
    {
        UnSubscribePlayerEvents();
        base.OnDisable();
    }
    protected override void Start()
    {
        base.Start();
        StartTimer();
    }
    protected void Update()
    {
        if(!isRunning) return;

        elapsedTime += Time.deltaTime;//chạy theo frame với lại khi dừng để upgrade thì time sẽ ko chạy
        TimeChanged?.Invoke(elapsedTime);
    }
    protected void SubscribePlayerEvents()
    {
        if(playerHealth == null) return;
        playerHealth.Died += HandlePlayerDied;
    }
    protected void UnSubscribePlayerEvents()
    {
        if(playerHealth == null) return;
        playerHealth.Died -= HandlePlayerDied;
    }
    protected void StartTimer()
    {
        elapsedTime = 0f;
        isRunning = true;

        TimeChanged?.Invoke(elapsedTime);//phải phát thì UI mới reset timer về 0 được còn ko thì sẽ sài trong inspector
    }
    protected void HandlePlayerDied()
    {
        StopTimer();
    }
    protected void StopTimer()
    {
        if(!isRunning) return;
        isRunning = false;

        TimeChanged?.Invoke(elapsedTime);//đảm bảo UI nhận đúng giá trị cuối cùng
        SurvivalTime?.Invoke(elapsedTime);//thông báo lượt chơi đã kết thúc
    }
}
