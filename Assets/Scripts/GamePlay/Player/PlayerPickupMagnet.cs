using UnityEngine;

public class PlayerPickupMagnet : HaiMonoBehaviour
{
    [SerializeField] private float attractRadius = 2.5f;

    public float AttractRadius => attractRadius;

    protected override void ResetValues()
    {
        base.ResetValues();
        attractRadius = 2.5f;
    }

    public void AddAttractRadius(float amount)
    {
        if (amount <= 0f) return;
        attractRadius += amount;
    }
}
