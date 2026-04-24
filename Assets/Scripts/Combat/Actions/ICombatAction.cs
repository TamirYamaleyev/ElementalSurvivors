public interface ICombatAction
{
    CombatResult Execute(CombatEntity actor, CombatEntity target, BGMType floorType);
}
