using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class PracticeXPMagnetDetector : HaiMonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform playerTarget;

    [SerializeField]
    private PracticePlayerExperience playerExperience;

    [SerializeField]
    private CircleCollider2D magnetCollider;

    public float MagnetRadius =>
        magnetCollider != null ? magnetCollider.radius : 0f;

    protected override void LoadComponents()
    {
        base.LoadComponents();

        if (playerExperience == null)
        {
            playerExperience = GetComponentInParent<PracticePlayerExperience>();
        }

        if (magnetCollider == null)
        {
            magnetCollider = GetComponent<CircleCollider2D>();
        }

        if (playerExperience != null && playerTarget == null)
        {
            playerTarget = playerExperience.transform;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        LoadComponents();
    }

    public void AddMagnetRadius(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        if (magnetCollider == null)
        {
            magnetCollider = GetComponent<CircleCollider2D>();
        }

        if (magnetCollider == null)
        {
            return;
        }

        magnetCollider.radius += amount;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (playerTarget == null) return;

        PracticeXPAttractable xPAttractable =
            other.GetComponentInParent<PracticeXPAttractable>();

        if (xPAttractable == null) return;

        xPAttractable.SetAttractionTarget(playerTarget);
    }

    private void OnDrawGizmosSelected()
    {
        if (magnetCollider == null)
        {
            magnetCollider = GetComponent<CircleCollider2D>();
        }

        if (magnetCollider == null) return;
        Gizmos.color = Color.green;

        Vector3 magnerCenter = magnetCollider.bounds.center;
        float worldRadius = magnetCollider.bounds.extents.x;

        Gizmos.DrawWireSphere(magnerCenter, worldRadius);
    }
}