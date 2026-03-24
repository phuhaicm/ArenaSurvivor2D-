using UnityEngine;

public abstract class HaiMonoBehaviour : MonoBehaviour
{
    protected virtual void Reset()
    {
        LoadComponents();
        ResetValues();
    }

    protected virtual void Awake()
    {
        LoadComponents();
    }

    protected virtual void Start()
    {
    }

    protected virtual void OnEnable()
    {
    }

    protected virtual void OnDisable()
    {
    }

    protected virtual void LoadComponents()
    {
    }

    protected virtual void ResetValues()
    {
    }

    protected void LogLoad(string methodName)
    {
        Debug.Log($"{name}: {methodName}", gameObject);
    }
}
