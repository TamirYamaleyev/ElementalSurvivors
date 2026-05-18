using UnityEngine;

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
        switch (definition.behaviorType)
        {
            case WeaponBehaviorType.Projectile:
                Vector2 pos = ctx.ProjectileSpawnPoint.position;
                Quaternion rot = ctx.ProjectileSpawnPoint.rotation;

                ctx.ProjectileSystem.Fire(
                    definition.projectilePrefab,
                    pos,
                    rot,
                    data.damage,
                    data.speed,
                    definition.appliedStatus,
                    data.statusDuration,
                    ctx.StatusSystem
                    );
                break;

            case WeaponBehaviorType.Area:
                if (target == null)
                    return;

                ctx.AreaSystem.Cast(
                    target.transform.position,
                    data.range,
                    data.damage,
                    definition.appliedStatus,
                    data.statusDuration,
                    ctx.StatusSystem
                    );
                break;

            case WeaponBehaviorType.Orbit:
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

        //ctx.StatusSystem.Apply(target, definition.appliedStatus, Current.damage);
    }
}
