using UnityEngine;

public enum WeaponBehaviorType
{
    Projectile,
    Area,
    Orbit,
    Custom
}

[System.Serializable]
public struct ElementUIData
{
    public string name;
    public Color color;
}

[CreateAssetMenu(fileName = "WeaponDefinition", menuName = "Scriptable Objects/WeaponDefinition")]
public class WeaponDefinition : ScriptableObject
{
    public string weaponName;
    public Sprite icon;
    public ElementUIData element;

    [TextArea]
    public string description;

    public WeaponBehaviorType behaviorType;
    public StatusType appliedStatus;

    public Projectile projectilePrefab;
    public OrbitingObject orbitPrefab;
    public AreaWeapon areaPrefab;
    public WeaponBehavior customWeaponPrefab;

    public WeaponLevelData[] levels = new WeaponLevelData[5];
}
