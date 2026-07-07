using UnityEngine;

public class PassiveUpgradeOption : UpgradeOption
{
    private readonly PassiveInstance passive;
    private readonly PassiveSystem passiveSystem;

    public PassiveUpgradeOption(PassiveInstance passive, PassiveSystem passiveSystem)
    {
        this.passive = passive;
        this.passiveSystem = passiveSystem;
    }

    public override Sprite Icon => passive.definition.icon;

    public override string Name => passive.definition.name;

    public override string Description => passive.definition.description;

    public override string LevelText => $"Level {passive.level} -> {passive.level + 1}";

    public override ElementUIData Element => default;

    public override void Apply()
    {
        passiveSystem.UpgradePassive(passive);
    }
}
