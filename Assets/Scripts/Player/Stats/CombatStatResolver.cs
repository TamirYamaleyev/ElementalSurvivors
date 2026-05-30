using UnityEngine;

public static class CombatStatResolver
{
    public static float ScaleDamage(float weaponDamage, in PlayerStatsSnapshot stats)
    {
        return weaponDamage * stats.DamageMultiplier;
    }

    public static float ScaleCooldown(float weaponCooldown, in PlayerStatsSnapshot stats)
    {
        return weaponCooldown / Mathf.Max(stats.AttackSpeed, 0.01f);
    }

    public static float ScaleProjectileSpeed(float weaponSpeed, in PlayerStatsSnapshot stats)
    {
        return weaponSpeed * stats.ProjectileSpeedMultiplier;
    }
}
