using TMPro;
using UnityEngine;

/// <summary>
/// World-space label for reaction VFX showcase enemies.
/// </summary>
public static class ReactionShowcaseLabel
{
    private const float LabelHeight = 1.15f;
    private const float FontSize = 2.2f;
    private const int SortingOrder = 50;

    public static void Create(Transform anchor, string reactionName, StatusType a, StatusType b)
    {
        if (anchor == null)
            return;

        var go = new GameObject("ReactionLabel");
        go.transform.SetParent(anchor, false);
        go.transform.localPosition = new Vector3(0f, LabelHeight, 0f);

        var text = go.AddComponent<TextMeshPro>();
        text.text = $"{reactionName}\n{a} + {b}";
        text.fontSize = FontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.sortingOrder = SortingOrder;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.rectTransform.sizeDelta = new Vector2(3f, 1.5f);
    }
}
