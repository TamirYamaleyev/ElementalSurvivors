using UnityEngine;

public abstract class WeaponBehavior : MonoBehaviour
{
    public abstract bool Execute(Enemy target, WeaponLevelData data, WeaponSystemContext ctx, WeaponDefinition definition);
}
