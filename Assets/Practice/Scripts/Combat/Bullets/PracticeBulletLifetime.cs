using UnityEngine;

public class PracticeBulletLifetime : HaiMonoBehaviour, IPracticePoolable
{
    [SerializeField]
    protected float timeLife = 2f;

    protected float destroyTime;

    protected override void Awake()
    {
        base.Awake();
        ResetLifetimeTimer();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        ResetLifetimeTimer();
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
            PracticeObjectPoolManager.DespawnOrDestroy(gameObject);// bộ đếm 2s thu hồi về kho
        }
    }

    private void ResetLifetimeTimer()
    {
        destroyTime = Time.time + timeLife;
    }

    public void OnTakenFromPool()
    {
        ResetLifetimeTimer();
    }

    public void OnReturnedToPool()
    {
        destroyTime = 0f;
    }

    public void ResetRuntimeState()
    {
        ResetLifetimeTimer();
    }
}