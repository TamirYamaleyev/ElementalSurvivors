public class AttackAction : ICombatAction
{
    private readonly int basePower;
    private readonly AttackType attackType;

    public AttackAction(int basePower = 0, AttackType attackType = AttackType.Neutral)
    {
        this.basePower = basePower;
        this.attackType = attackType;
    }

    public CombatResult Execute(CombatEntity actor, CombatEntity target, BGMType floorType)
    {
        CombatResult result = ActionResolver.ResolveDamageAction(actor, target, basePower, attackType, floorType);
        result.ActionLabel = "Attack";
        return result;
    }
}
