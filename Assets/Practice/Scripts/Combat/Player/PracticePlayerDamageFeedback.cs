using UnityEngine;

public class PracticePlayerDamageFeedback
    : HaiMonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PracticePlayerHealth playerHealth;

    [SerializeField]
    private PracticeSpriteFlashFeedback flashFeedback;

    protected override void LoadComponents()
    {
        base.LoadComponents();

        LoadPlayerHealth();
        LoadFlashFeedback();
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

    private void LoadFlashFeedback()
    {
        if (flashFeedback != null)
        {
            return;
        }

        flashFeedback =
            GetComponent<PracticeSpriteFlashFeedback>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        LoadPlayerHealth();
        LoadFlashFeedback();

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

        playerHealth.DamageTaken -=
            HandleDamageTaken;

        playerHealth.DamageTaken +=
            HandleDamageTaken;
    }

    private void UnsubscribePlayerEvents()
    {
        if (playerHealth == null)
        {
            return;
        }

        playerHealth.DamageTaken -=
            HandleDamageTaken;
    }

    private void HandleDamageTaken(
        float damageAmount
    )
    {
        flashFeedback?.PlayFlash();
    }
}