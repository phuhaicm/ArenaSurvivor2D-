using UnityEngine;

public static class ResourcePrefabLoader
{
    public static T LoadPrefab<T>(string resourcePath) where T : Component
    {
        T prefab = Resources.Load<T>(resourcePath);

        if (prefab == null)
        {
            Debug.LogError($"Failed to load prefab at Resources path: {resourcePath}");
        }

        return prefab;
    }
}