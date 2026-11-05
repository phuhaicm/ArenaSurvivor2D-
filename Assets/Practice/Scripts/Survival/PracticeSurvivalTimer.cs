using System;
using UnityEngine;

public class PracticeSurvivalTimer : HaiMonoBehaviour
{
    [Header("References")]
    [SerializeField] private PracticePlayerHealth playerHealth;
    [SerializeField] private PracticeGameStateController gameStateController;
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
        LoadGameStateController();
    }

    protected void LoadPlayerHealth()
    {
        if (playerHealth != null)
        {
            return;
        }

        playerHealth = FindFirstObjectByType<PracticePlayerHealth>();
    }

    private void LoadGameStateController()
    {
        if (gameStateController != null)
        {
            return;
        }

        gameStateController = FindFirstObjectByType<PracticeGameStateController>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        LoadPlayerHealth();
        LoadGameStateController();
        SubscribePlayerEvents();
        SubscribeGameStateEvents();
    }

    protected override void OnDisable()
    {
        UnSubscribePlayerEvents();
        UnsubscribeGameStateEvents();
        base.OnDisable();
    }

    protected override void Start()
    {
        base.Start();
        StartTimer();
    }

    protected void Update()
    {
        if (!isRunning) return;

        // Guard Clause: Khi game đang Pause hoặc chọn Upgrade (Time.timeScale = 0 -> Time.deltaTime = 0),
        // không phát sự kiện TimeChanged vô ích mỗi khung hình.
        if (Time.deltaTime <= 0f) return;

        elapsedTime += Time.deltaTime;
        TimeChanged?.Invoke(elapsedTime);
    }

    protected void SubscribePlayerEvents()
    {
        if (playerHealth == null) return;
        playerHealth.Died -= HandlePlayerDied;
        playerHealth.Died += HandlePlayerDied;
    }

    protected void UnSubscribePlayerEvents()
    {
        if (playerHealth == null) return;
        playerHealth.Died -= HandlePlayerDied;
    }

    private void SubscribeGameStateEvents()
    {
        if (gameStateController == null) return;
        gameStateController.StateChanged -= HandleGameStateChanged;
        gameStateController.StateChanged += HandleGameStateChanged;
    }

    private void UnsubscribeGameStateEvents()
    {
        if (gameStateController == null) return;
        gameStateController.StateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(PracticeGameStates newState)
    {
        if (newState == PracticeGameStates.GameOver || newState == PracticeGameStates.Victory)
        {
            StopTimer();
        }
    }

    protected void StartTimer()
    {
        elapsedTime = 0f;
        isRunning = true;

        TimeChanged?.Invoke(elapsedTime);
    }

    protected void HandlePlayerDied()
    {
        StopTimer();
    }

    protected void StopTimer()
    {
        if (!isRunning) return;
        isRunning = false;

        TimeChanged?.Invoke(elapsedTime);
        SurvivalTime?.Invoke(elapsedTime);
    }
}