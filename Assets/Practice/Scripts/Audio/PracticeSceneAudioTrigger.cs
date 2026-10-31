using UnityEngine;

public class PracticeSceneAudioTrigger : HaiMonoBehaviour
{
    [Header("Gameplay System References")]
    [SerializeField]
    private PracticeGameStateController gameStateController;

    [SerializeField]
    private PracticeRunDirector runDirector;

    [SerializeField]
    private PracticeFinalBossController finalBossController;

    private bool hasTriggeredBossMusic;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadGameplaySystems();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        hasTriggeredBossMusic = false;
        LoadGameplaySystems();
        SubscribeEvents();
    }

    protected override void OnDisable()
    {
        UnsubscribeEvents();
        base.OnDisable();
    }

    private void LoadGameplaySystems()
    {
        if (gameStateController == null)
        {
            gameStateController = FindFirstObjectByType<PracticeGameStateController>();
        }

        if (runDirector == null)
        {
            runDirector = FindFirstObjectByType<PracticeRunDirector>();
        }

        if (finalBossController == null)
        {
            finalBossController = FindFirstObjectByType<PracticeFinalBossController>();
        }
    }

    private void SubscribeEvents()
    {
        if (gameStateController != null)
        {
            gameStateController.StateChanged -= HandleGameStateChanged;
            gameStateController.StateChanged += HandleGameStateChanged;
        }

        if (runDirector != null)
        {
            runDirector.RunPhaseChanged -= HandleRunPhaseChanged;
            runDirector.RunPhaseChanged += HandleRunPhaseChanged;
        }

        if (finalBossController != null)
        {
            finalBossController.BossSpawned -= HandleBossSpawned;
            finalBossController.BossSpawned += HandleBossSpawned;
        }
    }

    private void UnsubscribeEvents()
    {
        if (gameStateController != null)
        {
            gameStateController.StateChanged -= HandleGameStateChanged;
        }

        if (runDirector != null)
        {
            runDirector.RunPhaseChanged -= HandleRunPhaseChanged;
        }

        if (finalBossController != null)
        {
            finalBossController.BossSpawned -= HandleBossSpawned;
        }
    }

    private void HandleRunPhaseChanged(PracticeRunPhase newPhase)
    {
        if (newPhase == PracticeRunPhase.BossFight)
        {
            TriggerBossMusicOnce();
        }
    }

    private void HandleBossSpawned()
    {
        TriggerBossMusicOnce();
    }

    private void TriggerBossMusicOnce()
    {
        if (hasTriggeredBossMusic)
        {
            return;
        }

        hasTriggeredBossMusic = true;
        PracticeAudioManager.Instance?.PlayBossMusic();
    }

    private void HandleGameStateChanged(PracticeGameStates newState)
    {
        if (PracticeAudioManager.Instance == null)
        {
            return;
        }

        if (newState == PracticeGameStates.GameOver)
        {
            PracticeAudioManager.Instance.StopMusic();
            PracticeAudioManager.Instance.PlayGameOverSfx();
        }
        else if (newState == PracticeGameStates.Victory)
        {
            PracticeAudioManager.Instance.StopMusic();
            PracticeAudioManager.Instance.PlayVictorySfx();
        }
    }
}