using UnityEngine;

public class PracticeBulletLifetime : HaiMonoBehaviour
{
    [SerializeField] protected float timeLife = 2f;

    protected float destroyTime;

    protected override void Awake()
    {
        base.Awake();
        destroyTime = Time.time + timeLife;
    }

    protected override void ResetValues()
    {
        base.ResetValues();
        timeLife = 2f;
    }

    private void Update()
    {
        if (Time.time >= destroyTime)
        {
            Destroy(gameObject);
        }
    }
}