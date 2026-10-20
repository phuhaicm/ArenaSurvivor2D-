using UnityEngine;

public class PracticeRunDirector : HaiMonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PracticeSurvivalTimer survivalTimer;

    [SerializeField]
    private PracticeGameStateController gameStateController;

    [Header("Run Settings")]
    [SerializeField, Min(1f)]
    private float runDuration = 5f;

    private bool victoryRequested;

    public float RunDuration =>
        runDuration;

    public float RemainingTime
    {
        get
        {
            if (survivalTimer == null)
            {
                return runDuration;
            }

            return Mathf.Max(
                0f,
                runDuration -
                survivalTimer.ElapsedTime
            );
        }
    }

    protected override void LoadComponents()
    {
        base.LoadComponents();

        LoadSurvivalTimer();
        LoadGameStateController();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        LoadSurvivalTimer();
        LoadGameStateController();

        victoryRequested = false;
    }

    protected override void Start()
    {
        base.Start();

        Debug.Log(
            $"{name}: Run Director started. " +
            $"Run Duration = {runDuration}, " +
            $"Timer = {survivalTimer}, " +
            $"Game State Controller = {gameStateController}.",
            this
        );
    }

    private void Update()
    {
        CheckVictoryCondition();
    }

    private void LoadSurvivalTimer()
    {
        if (survivalTimer != null)
        {
            return;
        }

        survivalTimer =
            FindFirstObjectByType<
                PracticeSurvivalTimer
            >();

        if (survivalTimer == null)
        {
            Debug.LogError(
                $"{name}: Không tìm thấy PracticeSurvivalTimer.",
                this
            );
        }
    }

    private void LoadGameStateController()
    {
        if (gameStateController != null)
        {
            return;
        }

        gameStateController =
            FindFirstObjectByType<
                PracticeGameStateController
            >();

        if (gameStateController == null)
        {
            Debug.LogError(
                $"{name}: Không tìm thấy " +
                $"PracticeGameStateController.",
                this
            );
        }
    }

    private void CheckVictoryCondition()
    {
        if (victoryRequested)
        {
            return;
        }

        if (survivalTimer == null ||
            gameStateController == null)
        {
            return;
        }

        if (!gameStateController.IsRunning)
        {
            return;
        }

        if (survivalTimer.ElapsedTime <
            runDuration)
        {
            return;
        }

        Debug.Log(
            $"{name}: Victory condition reached. " +
            $"Elapsed = {survivalTimer.ElapsedTime}, " +
            $"Required = {runDuration}.",
            this
        );

        RequestVictory();
    }

    private void RequestVictory()
    {
        bool changedSuccessfully =
            gameStateController.RequestVictory();

        Debug.Log(
            $"{name}: RequestVictory result = " +
            $"{changedSuccessfully}. " +
            $"Current State = " +
            $"{gameStateController.CurrentState}.",
            this
        );

        if (!changedSuccessfully &&
            !gameStateController.IsVictory)
        {
            Debug.LogWarning(
                $"{name}: Victory request was rejected.",
                this
            );

            return;
        }

        victoryRequested = true;
    }
}