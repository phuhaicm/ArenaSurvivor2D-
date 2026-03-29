using UnityEngine;

public static class UIHierarchyLookup
{
    public static T FindInParentCanvas<T>(Component requester) where T : Component
    {
        if (requester == null)
        {
            return null;
        }

        Canvas parentCanvas = requester.GetComponentInParent<Canvas>();

        if (parentCanvas == null)
        {
            Debug.LogWarning($"{requester.name}: Parent Canvas not found.");
            return null;
        }

        T[] matches = parentCanvas.GetComponentsInChildren<T>(true);

        if (matches == null || matches.Length == 0)
        {
            return null;
        }

        return matches[0];
    }
}