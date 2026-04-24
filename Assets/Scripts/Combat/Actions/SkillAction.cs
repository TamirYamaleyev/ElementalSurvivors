public class SkillAction : ICombatAction
{
    private readonly SkillDataSO skillData;

    public SkillAction(SkillDataSO skillData)
    {
        this.skillData = skillData;
    }

    public static bool CanAfford(CombatEntity actor, SkillDataSO data)
    {
        if (actor == null || data == null)
            return false;

        if (data.costType == SkillCostType.MP)
            return actor.CurrentMP >= data.costValue;
        return actor.CurrentHP >= data.costValue;
    }

    public CombatResult Execute(CombatEntity actor, CombatEntity target, BGMType floorType)
    {
        if (skillData == null)
            return new CombatResult { ActionLabel = "Skill (Missing)" };

        if (!CanAfford(actor, skillData))
            return new CombatResult { ActionLabel = skillData.skillName, WasSkipped = true };

        if (skillData.costType == SkillCostType.MP)
            actor.ConsumeMP(skillData.costValue);
        else
            actor.ConsumeHP(skillData.costValue);

        CombatResult result = ActionResolver.ResolveDamageAction(actor, target, skillData.basePower, skillData.attackType, floorType);
        result.ActionLabel = skillData.skillName;
        result.WasSkipped = false;
        return result;
    }
}
