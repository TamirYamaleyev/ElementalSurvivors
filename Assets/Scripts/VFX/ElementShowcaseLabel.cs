using UnityEngine;

/// <summary>
/// World-space element name label for the elemental VFX showcase.
/// </summary>
public static class ElementShowcaseLabel
{
    public static void Create(Transform anchor, string elementName)
    {
        ReactionShowcaseLabel.CreateSingleLine(anchor, elementName, "ElementLabel");
    }
}
