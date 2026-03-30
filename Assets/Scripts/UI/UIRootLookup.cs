using UnityEngine;

public static class UIRootLookup
{
    public static TRoot FindRootInCanvas<TRoot>(Component requester) where TRoot : Component
    {
        if (requester == null) return null;

        Canvas canvas = requester.GetComponentInParent<Canvas>();
        if (canvas == null) return null;

        TRoot[] roots = canvas.GetComponentsInChildren<TRoot>(true);
        if (roots == null || roots.Length == 0) return null;

        return roots[0];
    }

    public static TChild FindInRoot<TRoot, TChild>(Component requester)
        where TRoot : Component
        where TChild : Component
    {
        TRoot root = FindRootInCanvas<TRoot>(requester);
        if (root == null) return null;

        return root.GetComponentInChildren<TChild>(true);
    }
}
