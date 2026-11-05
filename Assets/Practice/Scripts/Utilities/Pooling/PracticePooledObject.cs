using UnityEngine;

public class PracticePooledObject : HaiMonoBehaviour
{
    [Header("Pool Identity (Read-Only)")]
    [SerializeField]
    private GameObject originPrefab;

    [SerializeField]
    private bool isInPool;

    private PracticeObjectPoolManager ownerPoolManager;
    private IPracticePoolable[] cachedPoolables;//bởi vì có tới 3 script gắn cùng nhau

    public GameObject OriginPrefab => originPrefab;
    public bool IsInPool => isInPool;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        CachePoolableComponents();
    }

    protected override void Awake()
    {
        base.Awake();
        CachePoolableComponents();//GetComponentsInChildren<IPracticePoolable>(true) gom tất cả các script đó vào mảng cachedPoolables đúng 1 lần lúc Awake(), giúp những lần bắn sau chỉ cần duyệt mảng có sẵn mà không tốn bộ nhớ gọi GetComponents lại.
    }

    private void CachePoolableComponents()
    {
        if (cachedPoolables != null && cachedPoolables.Length > 0)
        {
            return;
        }

        cachedPoolables = GetComponentsInChildren<IPracticePoolable>(true);
    }

    public void SetupPoolIdentity(
        GameObject prefabReference,
        PracticeObjectPoolManager poolManager
    )
    {
        originPrefab = prefabReference;
        ownerPoolManager = poolManager;
        CachePoolableComponents();
    }

    public void NotifyTakenFromPool()
    {
        isInPool = false;
        CachePoolableComponents();

        for (int i = 0; i < cachedPoolables.Length; i++)
        {
            if (cachedPoolables[i] != null)
            {
                cachedPoolables[i].ResetRuntimeState();
                cachedPoolables[i].OnTakenFromPool();
            }
        }
    }

    public void NotifyReturnedToPool()
    {
        isInPool = true;
        CachePoolableComponents();

        for (int i = 0; i < cachedPoolables.Length; i++)
        {
            if (cachedPoolables[i] != null)
            {
                cachedPoolables[i].OnReturnedToPool();
                cachedPoolables[i].ResetRuntimeState();
            }
        }
    }
}