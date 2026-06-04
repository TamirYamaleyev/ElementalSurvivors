using UnityEngine;

[CreateAssetMenu(fileName = "ElementVisualSet", menuName = "Elemental Survivors/Element Visual Set")]
public class ElementVisualSetSO : ScriptableObject, IElementVisualPalette
{
    [System.Serializable]
    public struct ElementVisualEntry
    {
        public StatusType element;
        public Color tint;
    }

    [SerializeField] private ElementVisualEntry[] entries =
    {
        new ElementVisualEntry { element = StatusType.Fire, tint = new Color(0.95f, 0.25f, 0.15f) },
        new ElementVisualEntry { element = StatusType.Water, tint = new Color(0.2f, 0.55f, 0.95f) },
        new ElementVisualEntry { element = StatusType.Wind, tint = new Color(0.25f, 0.85f, 0.35f) },
        new ElementVisualEntry { element = StatusType.Earth, tint = new Color(0.4f, 0.22f, 0.08f) },
        new ElementVisualEntry { element = StatusType.Lightning, tint = new Color(0.55f, 0.82f, 1f) }
    };

    public Color GetTint(StatusType type)
    {
        if (entries == null)
            return Color.magenta;

        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].element == type)
                return entries[i].tint;
        }

        return Color.magenta;
    }
}
