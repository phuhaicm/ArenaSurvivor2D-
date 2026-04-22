using UnityEngine;

public class XPMagnetAttract : HaiMonoBehaviour
{
    [SerializeField] private float attractMoveSpeed = 7f;

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
        attractMoveSpeed = 7f;
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
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        transform.position += direction * attractMoveSpeed * Time.deltaTime;
    }
}