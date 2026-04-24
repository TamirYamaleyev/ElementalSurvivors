public class EnemyCombatBrain
{
    private readonly AttackAction basicAttack = new AttackAction(0, AttackType.Neutral);

    public CombatResult TakeTurn(CombatEntity actor, CombatEntity playerTarget, BGMType floorType)
    {
        return basicAttack.Execute(actor, playerTarget, floorType);
    }
}
