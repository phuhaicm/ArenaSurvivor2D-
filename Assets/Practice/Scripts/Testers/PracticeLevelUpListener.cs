using UnityEngine;

public class PracticeLevelUpListener : HaiMonoBehaviour
{
    [SerializeField]
    protected PracticePlayerLevelSystem levelSystem;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadLevelSystem();
    }

    protected override void Awake()
    {
        base.Awake();

#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        enabled = false;
        return;
#endif
    }

    protected virtual void LoadLevelSystem()
    {
        if (levelSystem != null) return;

        levelSystem = GetComponent<PracticePlayerLevelSystem>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SubscribeEvents();
    }

    protected override void OnDisable()
    {
        UnsubscribeEvents();
        base.OnDisable();
    }

    protected virtual void SubscribeEvents()
    {
        if (levelSystem == null) return;

        levelSystem.LevelChanged -= HandleLeveledUp;
        levelSystem.LevelChanged += HandleLeveledUp;
    }

    protected virtual void UnsubscribeEvents()
    {
        if (levelSystem == null) return;

        levelSystem.LevelChanged -= HandleLeveledUp;
    }

    protected virtual void HandleLeveledUp(float newLevel)
    {
        Debug.Log(
            $"Received Level Up Event | New Level: {newLevel}",
            gameObject
        );
    }
}