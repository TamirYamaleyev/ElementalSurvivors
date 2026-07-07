using UnityEditor.UIElements;
using UnityEngine;

public abstract class UpgradeOption
{
    public abstract Sprite Icon { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract string LevelText { get; }
    public abstract ElementUIData Element { get; }
    public abstract void Apply();
}
