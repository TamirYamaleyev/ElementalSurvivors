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

        if (cooldownTimer <= 0f)
        {
            var data = Current;
            Execute(target, ctx, data);
            cooldownTimer = data.cooldown;
        }
    }

    public void Execute(Enemy target, WeaponSystemContext ctx, WeaponLevelData data)
    {
        Vector2 spawnPos = ctx.ProjectileSpawnPoint.position;

        switch (definition.behaviorType)
        {
            case WeaponBehaviorType.Projectile:
            {
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
                break;
            }
        }
    }

    private Vector2 ResolveDirection(Enemy target, Vector2 origin, WeaponSystemContext ctx)
    {
        if (target != null)
            return ((Vector2)target.transform.position - origin).normalized;

        return ctx.AimDirection.LastDirection;
    }
}
