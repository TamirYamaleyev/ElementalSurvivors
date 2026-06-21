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
        var stats = ctx.PlayerStats != null ? ctx.PlayerStats.Current : default;

        float damage = ctx.PlayerStats != null
            ? CombatStatResolver.ScaleDamage(data.damage, stats)
            : data.damage;

        float speed = ctx.PlayerStats != null
            ? CombatStatResolver.ScaleProjectileSpeed(data.speed, stats)
            : data.speed;

        Vector2 spawnPos = ctx.PlayerTransformPoint != null
            ? ctx.PlayerTransformPoint.position
            : transformFallback(ctx);

        switch (definition.behaviorType)
        {
            case WeaponBehaviorType.Projectile:
            {
                spawnPos = ctx.ProjectileSpawnPoint.position;
                Vector2 dir = ResolveDirection(target, spawnPos, ctx);

                ctx.ProjectileSystem.Fire(
                    definition.projectilePrefab,
                    spawnPos,
                    dir,
                    damage,
                    speed,
                    definition.appliedStatus,
                    data.statusDuration,
                    ctx.StatusSystem,
                    data.visualSprite);

                return true;
            }

            case WeaponBehaviorType.Area:
            {
                Vector2 pos = ctx.AreaSpawnPoint != null
                    ? ctx.AreaSpawnPoint.position
                    : spawnPos;

                ctx.AreaSystem.Cast(
                    definition.areaPrefab,
                    pos,
                    data.width,
                    data.height,
                    damage,
                    data.lifetime,
                    definition.appliedStatus,
                    data.statusDuration,
                    ctx.StatusSystem,
                    data.visualSprite);

                return true;
            }

            case WeaponBehaviorType.Orbit:
            {
                ctx.OrbitSystem.Spawn(
                    definition.orbitPrefab,
                    ctx.OrbitCenter,
                    data.projectileCount,
                    data.range,
                    speed,
                    damage,
                    definition.appliedStatus,
                    data.statusDuration,
                    ctx.StatusSystem,
                    data.visualSprite);

                return true;
            }

            case WeaponBehaviorType.Custom:
            {
                if (definition.customWeaponPrefab == null)
                    return false;

                return definition.customWeaponPrefab.TryExecute(target, data, ctx, definition);
            }
        }

        return false;
    }

    static Vector2 transformFallback(WeaponSystemContext ctx)
    {
        if (ctx.ProjectileSpawnPoint != null)
            return ctx.ProjectileSpawnPoint.position;

        return Vector2.zero;
    }

    static Vector2 ResolveDirection(Enemy target, Vector2 origin, WeaponSystemContext ctx)
    {
        if (target != null)
            return ((Vector2)target.transform.position - origin).normalized;

        if (ctx.AimDirection != null)
            return ctx.AimDirection.LastDirection;

        return Vector2.right;
    }
}
