using UnityEngine;

/// <summary>
/// World-space label for reaction VFX showcase enemies.
/// Uses built-in TextMesh to avoid TMP font initialization issues in the editor.
/// </summary>
public static class ReactionShowcaseLabel
{
    private const float LabelHeight = 1.15f;
    private const int SortingOrder = 50;
    private const int FontSize = 32;
    private const float CharacterSize = 0.08f;

    private static Font cachedFont;

    public static void Create(Transform anchor, string reactionName, StatusType a, StatusType b)
    {
        if (anchor == null)
            return;

        var go = new GameObject("ReactionLabel");
        go.transform.SetParent(anchor, false);
        go.transform.localPosition = new Vector3(0f, LabelHeight, 0f);

        var font = ResolveFont();
        if (font == null)
        {
            Object.Destroy(go);
            return;
        }

        var text = go.AddComponent<TextMesh>();
        text.font = font;
        text.text = $"{reactionName}\n{a} + {b}";
        text.fontSize = FontSize;
        text.characterSize = CharacterSize;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = Color.white;
        text.lineSpacing = 1.1f;

        ApplySorting(go, anchor);
    }

    private static Font ResolveFont()
    {
        if (cachedFont != null)
            return cachedFont;

        cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (cachedFont == null)
            cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        return cachedFont;
    }

    private static void ApplySorting(GameObject go, Transform anchor)
    {
        var renderer = go.GetComponent<MeshRenderer>();
        if (renderer == null)
            return;

        renderer.sortingOrder = SortingOrder;

        var sprite = anchor.GetComponentInChildren<SpriteRenderer>();
        if (sprite != null)
            renderer.sortingLayerID = sprite.sortingLayerID;
    }
}
