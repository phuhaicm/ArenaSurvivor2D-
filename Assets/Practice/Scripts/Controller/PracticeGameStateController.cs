using System;
using UnityEngine;

public class PracticeGameStateController : HaiMonoBehaviour
{
    [SerializeField] protected PracticeGameStates currentState;
    public event Action<PracticeGameStates> StateChanged;
    public PracticeGameStates CurrentState => currentState;
    public bool IsRunning => currentState == PracticeGameStates.Running;
    public bool IsPaused => currentState == PracticeGameStates.Paused;
    public bool IsChoosingUpgrade => currentState == PracticeGameStates.UpgradeChoices;
    public bool IsGameOver => currentState == PracticeGameStates.GameOver;
    protected override void Start()
    {
        base.Start();
        InitializeState();
    }
    protected void InitializeState()
    {
        currentState = PracticeGameStates.Running;
        ApplyTimeScale(currentState);

        StateChanged?.Invoke(currentState);
    }
    protected bool TryChangeState(PracticeGameStates nextState)
    {
        if(currentState == nextState) return false;
        if (!CanTransition(nextState))
        {
            return false;
        }

        PracticeGameStates previousState = currentState;
        currentState = nextState;
        ApplyTimeScale(currentState);
        Debug.Log($"{name}: Game state change from " + $"{previousState} to {currentState}.", this);

        StateChanged?.Invoke(currentState);
        return true;
    }
    protected bool CanTransition(PracticeGameStates nextState)
    {
        if (nextState == PracticeGameStates.GameOver)//đoạn này ở trong vì ngoại lệ kiểu dù đang ở trạng thái nào đi nữa thì đều có thể chuyển sang trạng thái over
        {
            return currentState != PracticeGameStates.GameOver;
        }
        if (currentState == PracticeGameStates.GameOver) return false;// đoạn này là chặn mọi chuyển đổi rời khỏi over

        switch (currentState)
        {
            case PracticeGameStates.Running:
                {
                    return nextState == PracticeGameStates.UpgradeChoices || nextState == PracticeGameStates.Paused;
                }
            case PracticeGameStates.UpgradeChoices:
                {
                    return nextState == PracticeGameStates.Running;
                }
            case PracticeGameStates.Paused:
                {
                    return nextState == PracticeGameStates.Running;
                }
            default:
            return false;
        }
    }
    protected void ApplyTimeScale(PracticeGameStates state)
    {
        switch (state)
        {
            case PracticeGameStates.Running:
                {
                    Time.timeScale = 1f;
                    break;
                }
            case PracticeGameStates.UpgradeChoices:
            case PracticeGameStates.Paused:
            case PracticeGameStates.GameOver:
                {
                    Time.timeScale = 0f;
                    break;
                }
        }
    }
    public bool RequestRunning()
    {
        return TryChangeState(PracticeGameStates.Running);
    }
    public bool RequestUpgradeChoices()
    {
        return TryChangeState(PracticeGameStates.UpgradeChoices);
    }
    public bool RequestPaused()
    {
        return TryChangeState(PracticeGameStates.Paused);
    }
    public bool RequestGameOver()
    {
        return TryChangeState(PracticeGameStates.GameOver);
    }
}
