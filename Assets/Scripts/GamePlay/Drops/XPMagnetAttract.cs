using UnityEngine;

public class XPMagnetAttract : HaiMonoBehaviour
{
    [SerializeField] private float minAttractSpeed = 5f;
    [SerializeField] private float maxAttractSpeed = 14f;

    private PlayerPickupMagnet playerPickupMagnet;
    private Transform playerTransform;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadPlayerPickupMagnet();
        LoadPlayerTransform();
    }

    protected override void ResetValues()
    {
        base.ResetValues();
        minAttractSpeed = 5f;
        maxAttractSpeed = 14f;
    }

    private void Update()
    {
        if (!CanAttract())
        {
            return;
        }

        AttractTowardsPlayer();
    }

    private void LoadPlayerPickupMagnet()
    {
        if (playerPickupMagnet != null) return;
        playerPickupMagnet = FindFirstObjectByType<PlayerPickupMagnet>();
    }

    private void LoadPlayerTransform()
    {
        if (playerTransform != null) return;

        PlayerRoot playerRoot = FindFirstObjectByType<PlayerRoot>();
        if (playerRoot == null) return;

        playerTransform = playerRoot.transform;
    }

    private bool CanAttract()
    {
        if (playerPickupMagnet == null || playerTransform == null)
        {
            return false;
        }

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        return distance <= playerPickupMagnet.AttractRadius;
    }

    private void AttractTowardsPlayer()
    {
        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = playerTransform.position;

        float radius = Mathf.Max(0.01f, playerPickupMagnet.AttractRadius);
        float distance = Vector2.Distance(currentPosition, targetPosition);

        float normalized = 1f - Mathf.Clamp01(distance / radius);
        float currentSpeed = Mathf.Lerp(minAttractSpeed, maxAttractSpeed, normalized);

        transform.position = Vector3.MoveTowards(
            currentPosition,
            targetPosition,
            currentSpeed * Time.deltaTime
        );
    }
}