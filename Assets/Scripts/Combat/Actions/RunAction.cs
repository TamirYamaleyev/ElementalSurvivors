public class RunAction : ICombatAction
{
    public CombatResult Execute(CombatEntity actor, CombatEntity target, BGMType floorType)
    {
        CombatResult result = ActionResolver.ResolveRun(actor, target);
        result.ActionLabel = "Run";
        return result;
    }
}
