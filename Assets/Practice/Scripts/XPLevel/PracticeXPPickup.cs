using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class PracticeXPPickup : HaiMonoBehaviour
{
    [SerializeField] protected PracticeXPOrb xpOrb;
    private bool hasBeenCollected;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadXPOrb();
    }

    protected virtual void LoadXPOrb()
    {
        if (xpOrb != null) return;

        xpOrb = GetComponent<PracticeXPOrb>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PracticePlayerExperience playerExperience = other.GetComponentInParent<PracticePlayerExperience>();

        if(playerExperience == null) return;
        
        Collect(playerExperience);
    }
    public void Collect(PracticePlayerExperience playerExperience)
    {
        if(hasBeenCollected) return;
        if(playerExperience == null) return;

        hasBeenCollected = true;
        playerExperience.AddExperience(xpOrb.ExperienceAmount);
        Destroy(gameObject);
    }
    private void OnDrawGizmosSelected()
    {
        CircleCollider2D collider = GetComponent<CircleCollider2D>();

        if(collider == null) return;

        Gizmos.color = Color.yellow;
        Vector3 magnerCenter = collider.bounds.center;
        float worldRadius = collider.bounds.extents.x;

        Gizmos.DrawWireSphere(magnerCenter, worldRadius);

    }
}