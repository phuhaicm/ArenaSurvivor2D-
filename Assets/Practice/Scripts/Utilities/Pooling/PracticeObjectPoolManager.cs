using System;
using System.Collections.Generic;
using UnityEngine;

public class PracticeObjectPoolManager : HaiMonoBehaviour
{
    [Serializable]
    public struct PoolPrewarmEntry
    {
        public GameObject prefab;
        [Min(1)]
        public int initialCount;
        public Transform defaultContainer;
    }

    public static PracticeObjectPoolManager Instance { get; private set; }

    [Header("Prewarm Settings")]
    [SerializeField]
    private List<PoolPrewarmEntry> prewarmEntries =
        new List<PoolPrewarmEntry>();

    [Header("Fallback Container")]
    [SerializeField]
    private Transform fallbackContainer;

    private readonly Dictionary<GameObject, Queue<PracticePooledObject>> poolDictionary =
        new Dictionary<GameObject, Queue<PracticePooledObject>>();//Dùng chính GameObject của Prefab làm khóa (Key) và một hàng đợi Queue<PracticePooledObject> làm kho chứa (Value).Khi cần lấy đạn ra bắn, queue.Dequeue() lấy phần tử đầu hàng với tốc độ $O(1)$; khi đạn bay xong thu hồi về, queue.Enqueue() xếp vào cuối hàng cũng với tốc độ $O(1)$.

    private readonly Dictionary<GameObject, Transform> defaultContainers =
        new Dictionary<GameObject, Transform>();

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();

        if (fallbackContainer == null)
        {
            fallbackContainer = transform;
        }

        PrewarmConfiguredPools();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void PrewarmConfiguredPools()//Ngay khi bắt đầu màn chơi (Awake), hàm này đọc danh sách prewarmEntries trên Inspector để tạo sẵn 40 viên đạn Player và 60 viên đạn Boss rồi tắt đi (SetActive(false)) cất vào kho. Nhờ vậy trong lúc giao chiến căng thẳng, game không phải gọi Instantiate cấp phát bộ nhớ mới.
    {
        if (prewarmEntries == null)
        {
            return;
        }

        for (int i = 0; i < prewarmEntries.Count; i++)
        {
            PoolPrewarmEntry entry = prewarmEntries[i];

            if (entry.prefab == null || entry.initialCount <= 0)
            {
                continue;
            }

            if (entry.defaultContainer != null)
            {
                defaultContainers[entry.prefab] = entry.defaultContainer;
            }

            PrewarmPool(
                entry.prefab,
                entry.initialCount,
                entry.defaultContainer
            );
        }
    }

    public void PrewarmPool(
        GameObject prefab,
        int count,
        Transform container = null
    )
    {
        if (prefab == null || count <= 0)
        {
            return;
        }

        Queue<PracticePooledObject> queue = GetOrCreateQueue(prefab);
        Transform targetParent = ResolveContainer(prefab, container);

        for (int i = 0; i < count; i++)
        {
            PracticePooledObject instance =
                CreateNewInstance(prefab, targetParent);

            instance.NotifyReturnedToPool();
            instance.gameObject.SetActive(false);
            queue.Enqueue(instance);
        }
    }

    public GameObject SpawnGameObject(
        GameObject prefab,
        Vector3 position,
        Quaternion rotation,
        Transform parentContainer = null
    )
    {
        if (prefab == null)
        {
            return null;
        }

        Queue<PracticePooledObject> queue = GetOrCreateQueue(prefab);
        Transform targetParent = ResolveContainer(prefab, parentContainer);
        PracticePooledObject pooledInstance = null;

        while (queue.Count > 0 && pooledInstance == null)
        {
            pooledInstance = queue.Dequeue();
        }

        if (pooledInstance == null)
        {
            pooledInstance = CreateNewInstance(prefab, targetParent);
        }

        Transform instanceTransform = pooledInstance.transform;

        if (targetParent != null && instanceTransform.parent != targetParent)
        {
            instanceTransform.SetParent(targetParent, false);
        }

        instanceTransform.SetPositionAndRotation(position, rotation);
        pooledInstance.gameObject.SetActive(true);
        pooledInstance.NotifyTakenFromPool();

        return pooledInstance.gameObject;
    }

    public T Spawn<T>(
        T prefabComponent,
        Vector3 position,
        Quaternion rotation,
        Transform parentContainer = null
    ) where T : Component
    {
        if (prefabComponent == null)
        {
            return null;
        }

        GameObject spawnedObj = SpawnGameObject(
            prefabComponent.gameObject,
            position,
            rotation,
            parentContainer
        );

        return spawnedObj != null ? spawnedObj.GetComponent<T>() : null;
    }

    public void Despawn(GameObject instanceObject)
    {
        if (instanceObject == null)
        {
            return;
        }

        PracticePooledObject pooledObject =
            instanceObject.GetComponent<PracticePooledObject>();

        if (pooledObject == null || pooledObject.OriginPrefab == null)
        {
            Destroy(instanceObject);
            return;
        }

        if (pooledObject.IsInPool)
        {
            return;
        }

        Queue<PracticePooledObject> queue =
            GetOrCreateQueue(pooledObject.OriginPrefab);

        pooledObject.NotifyReturnedToPool();
        pooledObject.gameObject.SetActive(false);

        queue.Enqueue(pooledObject);
    }

    public void DespawnAllActiveInContainer(Transform container)
    {
        if (container == null)
        {
            return;
        }

        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Transform child = container.GetChild(i);

            if (child != null && child.gameObject.activeSelf)
            {
                Despawn(child.gameObject);
            }
        }
    }

    public static T SpawnOrInstantiate<T>(
        T prefabComponent,
        Vector3 position,
        Quaternion rotation,
        Transform parentContainer = null
    ) where T : Component
    {
        if (prefabComponent == null)
        {
            return null;
        }

        if (Instance != null)
        {
            return Instance.Spawn(
                prefabComponent,
                position,
                rotation,
                parentContainer
            );
        }

        return Instantiate(
            prefabComponent,
            position,
            rotation,
            parentContainer
        );
    }

    public static GameObject SpawnOrInstantiate(
        GameObject prefab,
        Vector3 position,
        Quaternion rotation,
        Transform parentContainer = null
    )
    {
        if (prefab == null)
        {
            return null;
        }

        if (Instance != null)
        {
            return Instance.SpawnGameObject(
                prefab,
                position,
                rotation,
                parentContainer
            );
        }

        return Instantiate(
            prefab,
            position,
            rotation,
            parentContainer
        );
    }

    public static void DespawnOrDestroy(GameObject instanceObject)
    {
        if (instanceObject == null)
        {
            return;
        }

        if (Instance != null)
        {
            Instance.Despawn(instanceObject);
            return;
        }

        Destroy(instanceObject);
    }

    private Queue<PracticePooledObject> GetOrCreateQueue(GameObject prefab)
    {
        if (!poolDictionary.TryGetValue(prefab, out Queue<PracticePooledObject> queue))
        {
            queue = new Queue<PracticePooledObject>();
            poolDictionary[prefab] = queue;
        }

        return queue;
    }

    private Transform ResolveContainer(GameObject prefab, Transform explicitContainer)
    {
        if (explicitContainer != null)
        {
            return explicitContainer;
        }

        if (prefab != null &&
            defaultContainers.TryGetValue(prefab, out Transform defaultParent) &&
            defaultParent != null)
        {
            return defaultParent;
        }

        return fallbackContainer;
    }

    private PracticePooledObject CreateNewInstance(
        GameObject prefab,
        Transform parentContainer
    )
    {
        GameObject newObj = Instantiate(
            prefab,
            Vector3.zero,
            Quaternion.identity,
            parentContainer
        );

        PracticePooledObject pooledComp =
            newObj.GetComponent<PracticePooledObject>();

        if (pooledComp == null)
        {
            pooledComp = newObj.AddComponent<PracticePooledObject>();
        }

        pooledComp.SetupPoolIdentity(prefab, this);
        return pooledComp;
    }
}