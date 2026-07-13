using UnityEngine;

public class WeaponInstance
{
    public WeaponDefinition definition;
    public int level;

    private float cooldownTimer;

    public WeaponLevelData Current => definition.levels[level - 1];

    public WeaponInstance(WeaponDefinition def, int startLevel = 1)
    {
        definition = def;
        level = Mathf.Max(1, startLevel);
        cooldownTimer = definition.behaviorType == WeaponBehaviorType.Orbit
            ? 0f
            : Current.cooldown;
    }

    public void Tick(float deltaTime, Enemy target, WeaponSystemContext ctx)
    {
        cooldownTimer -= deltaTime;

        if (cooldownTimer > 0f)
            return;

        var data = Current;

        bool fired;
        try
        {
            fired = TryExecute(target, ctx, data);
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
            fired = false;
        }

        if (!fired)
        {
            cooldownTimer = Mathf.Min(data.cooldown, 0.25f);
            return;
        }

        ctx.PlayerAnimation?.NotifyAttack();

        float baseCooldown = data.cooldown;
        cooldownTimer = ctx.PlayerStats != null
            ? CombatStatResolver.ScaleCooldown(baseCooldown, ctx.PlayerStats.Current)
            : baseCooldown;
    }

    public bool TryExecute(Enemy target, WeaponSystemContext ctx, WeaponLevelData data)
    {
        if (!WeaponExecutionStrategies.TryGet(definition.behaviorType, out var strategy))
            return false;

        var stats = ctx.PlayerStats != null ? ctx.PlayerStats.Current : default;

        float damage = ctx.PlayerStats != null
            ? CombatStatResolver.ScaleDamage(data.damage, stats)
            : data.damage;

        float speed = ctx.PlayerStats != null
            ? CombatStatResolver.ScaleProjectileSpeed(data.speed, stats)
            : data.speed;

        Vector2 spawnPos = WeaponExecutionUtility.ResolveSpawnFallback(ctx);

        return strategy.Execute(target, ctx, data, definition, damage, speed, spawnPos);
    }

    public bool TryLevelUp(int maxLevel)
    {
        if (maxLevel <= 0 || level >= maxLevel)
            return false;

        level++;
        cooldownTimer = definition.behaviorType == WeaponBehaviorType.Orbit
            ? 0f
            : Mathf.Min(cooldownTimer, Current.cooldown);
        return true;
    }
}
