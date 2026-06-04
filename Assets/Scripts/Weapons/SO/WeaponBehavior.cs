using UnityEngine;

public abstract class WeaponBehavior : MonoBehaviour
{
    public abstract bool TryExecute(Enemy target, WeaponLevelData data, WeaponSystemContext ctx, WeaponDefinition definition);
}
