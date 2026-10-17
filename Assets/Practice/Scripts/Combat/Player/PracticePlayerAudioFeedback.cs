using UnityEngine;

public class PracticePlayerAudioFeedback : HaiMonoBehaviour
{
    [SerializeField]
    protected PracticePlayerHealth playerHealth;

    protected override void LoadComponents()
    {
        base.LoadComponents();

        LoadPlayerHealth();
    }

    private void LoadPlayerHealth()
    {
        if (playerHealth != null)
        {
            return;
        }

        playerHealth =
            GetComponent<PracticePlayerHealth>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        LoadPlayerHealth();
        SubscribePlayerEvents();
    }

    protected override void OnDisable()
    {
        UnsubscribePlayerEvents();

        base.OnDisable();
    }

    private void SubscribePlayerEvents()
    {
        if (playerHealth == null)
        {
            return;
        }

        playerHealth.damageTaken +=
            HandlePlayerDamageTaken;
    }

    private void UnsubscribePlayerEvents()
    {
        if (playerHealth == null)
        {
            return;
        }

        playerHealth.damageTaken -=
            HandlePlayerDamageTaken;
    }

    private void HandlePlayerDamageTaken(float damageAmount)
    {
        PracticeAudioManager.Instance?.
            PlayPlayerHitSfx();
    }
}