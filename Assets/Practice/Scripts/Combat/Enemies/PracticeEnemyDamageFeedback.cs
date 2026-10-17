using UnityEngine;

public class PracticeEnemyDamageFeedback
    : HaiMonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PracticeEnemyHealth enemyHealth;

    [SerializeField]
    private PracticeSpriteFlashFeedback flashFeedback;

    protected override void LoadComponents()
    {
        base.LoadComponents();

        LoadEnemyHealth();
        LoadFlashFeedback();
    }

    private void LoadEnemyHealth()
    {
        if (enemyHealth != null)
        {
            return;
        }

        enemyHealth =
            GetComponent<PracticeEnemyHealth>();
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

        LoadEnemyHealth();
        LoadFlashFeedback();

        SubscribeEnemyEvents();
    }

    protected override void OnDisable()
    {
        UnsubscribeEnemyEvents();

        base.OnDisable();
    }

    private void SubscribeEnemyEvents()
    {
        if (enemyHealth == null)
        {
            return;
        }

        enemyHealth.damageTaken -=
            HandleDamageTaken;

        enemyHealth.damageTaken +=
            HandleDamageTaken;
    }

    private void UnsubscribeEnemyEvents()
    {
        if (enemyHealth == null)
        {
            return;
        }

        enemyHealth.damageTaken -=
            HandleDamageTaken;
    }

    private void HandleDamageTaken(
        float damageAmount
    )
    {
        flashFeedback?.PlayFlash();
    }
}