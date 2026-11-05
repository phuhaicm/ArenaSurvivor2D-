using UnityEngine;
[RequireComponent(typeof(CircleCollider2D))]

public class PracticeXPPickupReceiver : HaiMonoBehaviour
{
    PracticePlayerExperience playerExperience;
    protected override void LoadComponents()
    {
        base.LoadComponents();
        playerExperience = GetComponentInParent<PracticePlayerExperience>();//tại vì component nằm trên player và thể hiện rõ object này là object nhận kinh nghiệm nên chọn component này
    }
    private void OnTriggerEnter2D(Collider2D other) {
        if(playerExperience == null) return;

        PracticeXPPickup xpPickup = other.GetComponentInParent<PracticeXPPickup>();
        
        if(xpPickup == null) return;

        xpPickup.Collect(playerExperience);
    }
}
