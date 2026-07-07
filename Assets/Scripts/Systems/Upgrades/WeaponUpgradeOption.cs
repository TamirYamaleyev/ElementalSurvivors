using UnityEngine;

public class WeaponUpgradeOption
{
    public WeaponInstance weapon;

    public WeaponDefinition unlockDefinition;

    public bool IsUnlock => unlockDefinition != null;

    public WeaponUpgradeOption(WeaponInstance weapon)
    {
        this.weapon = weapon;
    }

    public WeaponUpgradeOption(WeaponDefinition definition)
    {
        unlockDefinition = definition;
    }
}
