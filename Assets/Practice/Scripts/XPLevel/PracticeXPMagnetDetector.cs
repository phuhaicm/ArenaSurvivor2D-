using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class PracticeXPMagnetDetector : HaiMonoBehaviour
{
    private Transform playerTarget;
    PracticePlayerExperience playerExperience;
    private CircleCollider2D magnetCollider;
    protected override void LoadComponents()
    {
        base.LoadComponents();
        
        playerExperience = GetComponentInParent<PracticePlayerExperience>();//tại vì component nằm trên player và thể hiện rõ object này là object nhận kinh nghiệm nên chọn component này
        magnetCollider = GetComponent<CircleCollider2D>();

        if(playerExperience != null)
        {
            playerTarget = playerExperience.transform;
        }
    }
    private void OnTriggerEnter2D(Collider2D other) {
        if(playerTarget == null) return;

        PracticeXPAttractable xPAttractable = other.GetComponentInParent<PracticeXPAttractable>();
        
        if(xPAttractable == null) return;

        xPAttractable.SetAttractionTarget(playerTarget);
    }
    private void OnDrawGizmosSelected()
    {
        if(magnetCollider == null)
        {
            magnetCollider = GetComponent<CircleCollider2D>();
        }

        if(magnetCollider == null) return;
        Gizmos.color = Color.green;

        Vector3 magnerCenter = magnetCollider.bounds.center;
        float worldRadius = magnetCollider.bounds.extents.x;

        Gizmos.DrawWireSphere(magnerCenter, worldRadius);

    }

}
