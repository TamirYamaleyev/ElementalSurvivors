using UnityEngine;

public class WeaponUpgradeOption : UpgradeOption
{
    private WeaponInstance weapon;
    private WeaponDefinition unlockDefinition;
    private WeaponSystem weaponSystem;

    public bool IsUnlock => unlockDefinition != null;

    public WeaponUpgradeOption(WeaponInstance weapon)
    {
        this.weapon = weapon;
    }

    public WeaponUpgradeOption(WeaponDefinition definition, WeaponSystem weaponSystem)
    {
        unlockDefinition = definition;
        this.weaponSystem = weaponSystem;
    }

    public override Sprite Icon => IsUnlock ? unlockDefinition.icon : weapon.definition.icon;

    public override string Name => IsUnlock ? unlockDefinition.weaponName : weapon.definition.weaponName;

    public override string Description => IsUnlock ? unlockDefinition.description : weapon.definition.description;

    public override string LevelText => IsUnlock ? "Unlock" : $"Level {weapon.level} -> {weapon.level + 1}";

    public override ElementUIData Element => IsUnlock ? unlockDefinition.element : weapon.definition.element;

    public override void Apply()
    {
        if (IsUnlock)
            weaponSystem.UnlockWeapon(unlockDefinition);
        else
            weapon.LevelUp();
    }


}
