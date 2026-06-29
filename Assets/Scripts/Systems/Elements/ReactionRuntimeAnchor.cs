using UnityEngine;

/// <summary>Always-active parent for in-game reaction gameplay roots (SampleScene, etc.).</summary>
public static class ReactionRuntimeAnchor
{
    public static Transform Root { get; private set; }

    public static void SetRoot(Transform root)
    {
        Root = root;
    }

    public static void ClearRoot(Transform root)
    {
        if (Root == root)
            Root = null;
    }
}
