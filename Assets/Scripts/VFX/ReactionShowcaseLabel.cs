using TMPro;
using UnityEngine;

/// <summary>
/// World-space label for reaction VFX showcase enemies.
/// </summary>
public static class ReactionShowcaseLabel
{
    private const float LabelHeight = 1.15f;
    private const int SortingOrder = 50;
    private const float FontSize = 3.2f;

    public static void Create(Transform anchor, string reactionName, StatusType a, StatusType b)
    {
        CreateLabel(anchor, $"{reactionName}\n{a} + {b}", "ReactionLabel");
    }

    public static void CreateSingleLine(Transform anchor, string labelText, string objectName = "ShowcaseLabel")
    {
        CreateLabel(anchor, labelText, objectName);
    }

    private static void CreateLabel(Transform anchor, string labelText, string objectName)
    {
        if (anchor == null)
            return;

        var go = new GameObject(objectName);
        go.transform.SetParent(anchor, false);
        go.transform.localPosition = new Vector3(0f, LabelHeight, 0f);

        var text = go.AddComponent<TextMeshPro>();
        text.text = labelText;
        text.fontSize = FontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.rectTransform.sizeDelta = new Vector2(6f, 2f);

        TmpFontUtility.EnsureAssigned(text);
        go.AddComponent<TmpFontOnEnable>();

        ApplySorting(go, anchor);
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
