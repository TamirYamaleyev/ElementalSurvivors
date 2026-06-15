using UnityEngine;

/// <summary>
/// Shared 2D world sprite setup so pickups and obstacles stay visible without per-scene light tuning.
/// </summary>
/// <remarks>
/// <para><b>Symptom: "mesh not applied" / nothing draws</b> — <see cref="SpriteRenderer"/> has no MeshFilter;
/// Unity generates a sprite quad mesh per frame. You still see zero contribution when: (1) Small Mesh Culling
/// drops sub-pixel meshes, (2) the sprite reference is missing, or (3) Lit materials receive no usable light.</para>
/// <para>World prefabs should use the same default Lit sprite material as the player (URP 2D Renderer default)
/// unless you have verified a custom unlit material in the target Unity version.</para>
/// <para><b>Verify in Unity</b>: Window → Analysis → Frame Debugger → enable → look for a draw for your sorting layer
/// and material; if there is no draw while the object is active, culling or a missing sprite is likely.</para>
/// </remarks>
public static class WorldSprite2DDefaults
{
    /// <summary>Optional project material: <c>Assets/Materials/M_SpriteWorldUnlit.mat</c> (verify in Editor before using on prefabs).</summary>
    public const string ProjectMaterialPath = "Assets/Materials/M_SpriteWorldUnlit.mat";

    /// <summary>
    /// Applies draw order for world sprites. Unlit material and Small Mesh Culling are set on prefabs (YAML),
    /// not overwritten here, so we do not replace a valid prefab material with a runtime instance that can
    /// behave differently across Unity versions.
    /// </summary>
    public static void Apply(SpriteRenderer renderer, int sortingOrder)
    {
        if (renderer == null)
            return;

        renderer.sortingOrder = sortingOrder;
        renderer.forceRenderingOff = false;
    }

    /// <summary>Applies <see cref="Apply"/> to every <see cref="SpriteRenderer"/> under <paramref name="root"/>.</summary>
    public static void ApplyToHierarchy(GameObject root, int sortingOrder)
    {
        if (root == null)
            return;
        foreach (var r in root.GetComponentsInChildren<SpriteRenderer>(true))
            Apply(r, sortingOrder);
    }

}
