using UnityEngine;

public class PlayerBulletLifetime : HaiMonoBehaviour
{
    [SerializeField] private float lifetime = 2f;

    private float destroyTime;

    protected override void Awake()
    {
        base.Awake();
        destroyTime = Time.time + lifetime;
    }

    protected override void ResetValues()
    {
        base.ResetValues();
        lifetime = 2f;
    }

    private void Update()
    {
        if (Time.time >= destroyTime)
        {
            Destroy(gameObject);
        }
    }
}