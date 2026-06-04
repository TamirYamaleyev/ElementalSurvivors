using UnityEngine;

public enum WeaponBehaviorType
{
    Projectile,
    Area,
    Orbit,
    Custom
}

[CreateAssetMenu(fileName = "WeaponDefinition", menuName = "Scriptable Objects/WeaponDefinition")]
public class WeaponDefinition : ScriptableObject
{
    public string weaponName;
    public WeaponBehaviorType behaviorType;
    public StatusType appliedStatus;

    public Projectile projectilePrefab;
    public OrbitingObject orbitPrefab;
    public WeaponBehavior customWeaponPrefab;

    public WeaponLevelData[] levels = new WeaponLevelData[5];
}
