public interface IStatusEffectTarget
{
    void ApplySlow(float duration, float multiplier);
    void ApplyFear(float duration);
    void ApplyDoT(float duration, float damagePerTick);
}
