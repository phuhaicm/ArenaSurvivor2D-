using UnityEngine;

public class GamePauseController : HaiMonoBehaviour
{
    private bool isPaused;

    public bool IsPaused => isPaused;

    protected override void Awake()
    {
        base.Awake();
        ForceResume();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        ForceResume();
    }

    protected override void ResetValues()
    {
        base.ResetValues();
        isPaused = false;
    }

    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        ForceResume();
    }

    private void ForceResume()
    {
        isPaused = false;
        Time.timeScale = 1f;
    }
}
