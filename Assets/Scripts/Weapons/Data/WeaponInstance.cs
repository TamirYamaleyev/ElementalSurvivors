using NUnit.Framework;
using UnityEngine;

[System.Serializable]
public class WeaponInstance
{
    public WeaponDefinition definition;
    public int level;

    private float cooldownTimer;

    public WeaponLevelData Current => definition.levels[level - 1];

    public WeaponInstance(WeaponDefinition def)
    {
        definition = def;
        level = 1;

        cooldownTimer = Current.cooldown;
    }

    public void Tick(float deltaTime, Enemy target, WeaponSystemContext ctx)
    {
        cooldownTimer -= deltaTime;

        if (cooldownTimer > 0f)
            return;

        var data = Current;

        bool fired = TryExecute(target, ctx, data);

        if (fired)
            cooldownTimer = data.cooldown;
    }

    public bool TryExecute(Enemy target, WeaponSystemContext ctx, WeaponLevelData data)
    {
        bool fired = false;

        Vector2 spawnPos = ctx.PlayerTransformPoint.position;

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
                    data.damage,
                    data.speed,
                    definition.appliedStatus,
                    data.statusDuration,
                    ctx.StatusSystem
                );

                fired = true;
                break;
            }

            case WeaponBehaviorType.Area:
            {

                Vector2 pos = ctx.AreaSpawnPoint.position;

                ctx.AreaSystem.Cast(
                    pos,
                    data.range,
                    data.damage,
                    definition.appliedStatus,
                    data.statusDuration,
                    ctx.StatusSystem
                );

                fired = true;
                break;
            }

            case WeaponBehaviorType.Orbit:
            {
                ctx.OrbitSystem.Spawn(
                    definition.orbitPrefab,
                    ctx.OrbitCenter,
                    data.projectileCount,
                    data.range,
                    data.speed,
                    data.damage,
                    definition.appliedStatus,
                    data.statusDuration,
                    ctx.StatusSystem
                );

                fired = true;
                break;
            }

            case WeaponBehaviorType.Custom:
            {
                fired = definition.customWeaponPrefab.TryExecute(target, data, ctx, definition);
                break;
            }
        }
        return fired;
    }

    private Vector2 ResolveDirection(Enemy target, Vector2 origin, WeaponSystemContext ctx)
    {
        if (target != null)
        {
            return ((Vector2)target.transform.position - origin).normalized;
        }

        return ctx.AimDirection.LastDirection;
    }
}
