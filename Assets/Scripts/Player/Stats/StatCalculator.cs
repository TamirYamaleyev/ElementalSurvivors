using System.Collections.Generic;

/// <summary>
/// Pure stat stacking: base * product(multipliers) + sum(flat additives).
/// CollectRadius uses base + sum(flat only); multipliers are ignored for that stat.
/// </summary>
public static class StatCalculator
{
    public static PlayerStatsSnapshot Compute(PlayerBaseStatsSO baseStats, IReadOnlyList<PlayerStatModifier> modifiers)
    {
        if (baseStats == null)
        {
            return new PlayerStatsSnapshot(0f, 0f, 0f, 0f, 0f, 0f);
        }

        return new PlayerStatsSnapshot(
            ResolveScaledStat(PlayerStatType.MaxHealth, baseStats.baseMaxHealth, modifiers),
            ResolveScaledStat(PlayerStatType.MoveSpeed, baseStats.baseMoveSpeed, modifiers),
            ResolveScaledStat(PlayerStatType.Damage, baseStats.baseDamageMultiplier, modifiers),
            ResolveScaledStat(PlayerStatType.AttackSpeed, baseStats.baseAttackSpeed, modifiers),
            ResolveScaledStat(PlayerStatType.ProjectileSpeed, baseStats.baseProjectileSpeedMultiplier, modifiers),
            ResolveCollectRadius(baseStats.baseCollectRadius, modifiers));
    }

    private static float ResolveScaledStat(PlayerStatType type, float baseValue, IReadOnlyList<PlayerStatModifier> modifiers)
    {
        float mult = 1f;
        float flat = 0f;

        for (int i = 0; i < modifiers.Count; i++)
        {
            var mod = modifiers[i];
            if (mod.stat != type)
                continue;

            if (mod.isMultiplier)
                mult *= mod.value;
            else
                flat += mod.value;
        }

        return baseValue * mult + flat;
    }

    private static float ResolveCollectRadius(float baseValue, IReadOnlyList<PlayerStatModifier> modifiers)
    {
        float flat = 0f;

        for (int i = 0; i < modifiers.Count; i++)
        {
            var mod = modifiers[i];
            if (mod.stat != PlayerStatType.CollectRadius)
                continue;

            if (!mod.isMultiplier)
                flat += mod.value;
        }

        return baseValue + flat;
    }
}
