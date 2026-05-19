public interface IStatModifierTarget : IPlayerStatsProvider
{
    void ApplyModifier(PlayerStatModifier modifier);
    void RemoveModifier(PlayerStatModifier modifier);
    void ClearModifiers();
}
