using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class WeaponExecutionStrategies
{
    private static readonly Dictionary<WeaponBehaviorType, IWeaponExecutionStrategy> Strategies = new()
    {
        { WeaponBehaviorType.Projectile, new ProjectileWeaponExecution() },
        { WeaponBehaviorType.Area, new AreaWeaponExecution() },
        { WeaponBehaviorType.Orbit, new OrbitWeaponExecution() },
        { WeaponBehaviorType.Custom, new CustomWeaponExecution() }
    };

    public static bool TryGet(WeaponBehaviorType behaviorType, out IWeaponExecutionStrategy strategy)
        => Strategies.TryGetValue(behaviorType, out strategy);
}

internal sealed class ProjectileWeaponExecution : IWeaponExecutionStrategy
{
    public bool Execute(
        Enemy target,
        WeaponSystemContext ctx,
        WeaponLevelData data,
        WeaponDefinition definition,
        float damage,
        float speed,
        Vector2 spawnPos)
    {
        spawnPos = ctx.ProjectileSpawnPoint.position;

        Vector2 playerPos = ctx.PlayerTransformPoint.position;
        Vector2 mousePos = ctx.AimDirection.MouseWorldPosition;

        Vector2 delta = mousePos - playerPos;

        Vector2 dir = delta.sqrMagnitude < 0.0001f ? Vector2.right : delta.normalized;

        //Vector2 dir = WeaponExecutionUtility.ResolveDirection(target, spawnPos, ctx);

        var directions = WeaponExecutionUtility.GenerateSpreadDirections(dir, data.projectileCount, data.spreadAngle);

        for (int i = 0; i < directions.Count; i++)
        {
            Vector2 offset = Vector2.Perpendicular(dir) * ((i - (directions.Count - 1) / 2f) * data.volleySpacing);

            ctx.ProjectileSystem.Fire(
                definition.projectilePrefab,
                spawnPos + offset,
                directions[i],
                damage,
                speed,
                definition.appliedStatus,
                data.statusDuration,
                ctx.StatusSystem,
                data.visualSpriteArr
            );
        }

        return true;
    }
}

internal sealed class AreaWeaponExecution : IWeaponExecutionStrategy
{
    public bool Execute(
        Enemy target,
        WeaponSystemContext ctx,
        WeaponLevelData data,
        WeaponDefinition definition,
        float damage,
        float speed,
        Vector2 spawnPos)
    {
        ctx.AreaSystem.Cast(
            definition.areaPrefab,
            ctx.AreaSpawnPoint,
            data.width,
            data.height,
            damage,
            data.lifetime,
            definition.appliedStatus,
            data.statusDuration,
            ctx.StatusSystem,
            data.visualSpriteArr);

        return true;
    }
}

internal sealed class OrbitWeaponExecution : IWeaponExecutionStrategy
{
    public bool Execute(
        Enemy target,
        WeaponSystemContext ctx,
        WeaponLevelData data,
        WeaponDefinition definition,
        float damage,
        float speed,
        Vector2 spawnPos)
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
            data.visualSpriteArr);

        return true;
    }
}

internal sealed class CustomWeaponExecution : IWeaponExecutionStrategy
{
    public bool Execute(
        Enemy target,
        WeaponSystemContext ctx,
        WeaponLevelData data,
        WeaponDefinition definition,
        float damage,
        float speed,
        Vector2 spawnPos)
    {
        if (definition.customWeaponPrefab == null)
            return false;

        return definition.customWeaponPrefab.TryExecute(target, data, ctx, definition);
    }
}

internal static class WeaponExecutionUtility
{
    public static List<Vector2> GenerateSpreadDirections(Vector2 baseDirection, int count, float spreadAngle)
    {
        List<Vector2> directions = new();

        if (count <= 1)
        {
            directions.Add(baseDirection.normalized);
            return directions;
        }

        float startAngle = -spreadAngle / 2f;
        float step = spreadAngle / (count - 1);

        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;

        for (int i = 0; i < count; i++)
        {
            float angle = baseAngle + startAngle + step * i;

            directions.Add(Quaternion.Euler(0, 0, angle) * Vector2.right);
        }

        return directions;
    }

    public static Vector2 ResolveSpawnFallback(WeaponSystemContext ctx)
    {
        if (ctx.PlayerTransformPoint != null)
            return ctx.PlayerTransformPoint.position;

        if (ctx.ProjectileSpawnPoint != null)
            return ctx.ProjectileSpawnPoint.position;

        return Vector2.zero;
    }

    public static Vector2 ResolveDirection(Enemy target, Vector2 origin, WeaponSystemContext ctx)
    {
        if (target != null)
            return ((Vector2)target.transform.position - origin).normalized;

        if (ctx.AimDirection != null)
            return ctx.AimDirection.LastDirection;

        return Vector2.right;
    }
}
