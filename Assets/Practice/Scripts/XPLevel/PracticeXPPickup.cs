using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class PracticeXPPickup : HaiMonoBehaviour, IPracticePoolable
{
    [SerializeField]
    protected PracticeXPOrb xpOrb;

    private bool hasBeenCollected;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadXPOrb();
    }

    protected override void Awake()
    {
        base.Awake();
        LoadXPOrb();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        LoadXPOrb();
        hasBeenCollected = false;
    }

    protected virtual void LoadXPOrb()
    {
        if (xpOrb != null) return;

        xpOrb = GetComponent<PracticeXPOrb>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasBeenCollected || other == null) return;

        PracticePlayerExperience playerExperience =
            other.GetComponentInParent<PracticePlayerExperience>();

        if (playerExperience == null) return;

        Collect(playerExperience);
    }

    public void Collect(PracticePlayerExperience playerExperience)
    {
        if (hasBeenCollected) return;
        if (playerExperience == null || xpOrb == null) return;

        hasBeenCollected = true;
        playerExperience.AddExperience(xpOrb.ExperienceAmount);

        // Thu hồi hạt XP về Object Pool thay vì Destroy(gameObject)
        PracticeObjectPoolManager.DespawnOrDestroy(gameObject);
    }

    public void OnTakenFromPool()
    {
        hasBeenCollected = false;
    }

    public void OnReturnedToPool()
    {
        hasBeenCollected = false;
    }

    public void ResetRuntimeState()
    {
        hasBeenCollected = false;
    }

    private void OnDrawGizmosSelected()
    {
        CircleCollider2D collider = GetComponent<CircleCollider2D>();

        if (collider == null) return;

        Gizmos.color = Color.yellow;
        Vector3 magnerCenter = collider.bounds.center;
        float worldRadius = collider.bounds.extents.x;

        Gizmos.DrawWireSphere(magnerCenter, worldRadius);
    }
}