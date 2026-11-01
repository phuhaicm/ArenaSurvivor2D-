using UnityEngine;

public class PracticeEnemyDamageFeedback
    : HaiMonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PracticeEnemyHealth enemyHealth;

    [SerializeField]
    private PracticeSpriteFlashFeedback flashFeedback;

    [Header("Floating Damage Text (Optional)")]
    [SerializeField]
    private PracticeFloatingDamageText damageTextPrefab;

    [SerializeField]
    private Transform damageTextContainer;

    protected override void LoadComponents()
    {
        base.LoadComponents();

        LoadEnemyHealth();
        LoadFlashFeedback();
        LoadDamageTextContainer();
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

    private void LoadDamageTextContainer()
    {
        if (damageTextContainer != null)
        {
            return;
        }

        GameObject containerObj = GameObject.Find("RuntimeDamageTexts");
        if (containerObj != null)
        {
            damageTextContainer = containerObj.transform;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        LoadEnemyHealth();
        LoadFlashFeedback();
        LoadDamageTextContainer();

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

        enemyHealth.DamageTaken -=
            HandleDamageTaken;

        enemyHealth.DamageTaken +=
            HandleDamageTaken;
    }

    private void UnsubscribeEnemyEvents()
    {
        if (enemyHealth == null)
        {
            return;
        }

        enemyHealth.DamageTaken -=
            HandleDamageTaken;
    }

    private void HandleDamageTaken(
        float damageAmount
    )
    {
        flashFeedback?.PlayFlash();
        SpawnFloatingDamageText(damageAmount);
    }

    private void SpawnFloatingDamageText(float damageAmount)
    {
        if (damageTextPrefab == null)
        {
            return;
        }

        LoadDamageTextContainer();

        PracticeFloatingDamageText spawnedText =
            PracticeObjectPoolManager.SpawnOrInstantiate(
                damageTextPrefab,
                transform.position,
                Quaternion.identity,
                damageTextContainer
            );

        if (spawnedText != null)
        {
            spawnedText.Initialize(damageAmount, transform.position);
        }
    }
}