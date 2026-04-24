public class DefendAction : ICombatAction
{
    public CombatResult Execute(CombatEntity actor, CombatEntity target, BGMType floorType)
    {
        CombatResult result = ActionResolver.ResolveDefend(actor);
        result.ActionLabel = "Defend";
        return result;
    }
}
