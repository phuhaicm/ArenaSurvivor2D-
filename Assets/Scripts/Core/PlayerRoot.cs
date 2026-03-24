using UnityEngine;

public class PlayerRoot : HaiMonoBehaviour
{
    private static PlayerRoot instance;
    public static PlayerRoot Instance => instance;

    protected override void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("Only one PlayerRoot is allowed to exist.", gameObject);
            return;
        }

        instance = this;
        base.Awake();
    }
}
