using System.Collections.Generic;
using UnityEngine;

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
        Vector2 dir = WeaponExecutionUtility.ResolveDirection(target, spawnPos, ctx);

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
            data.visualSprite);

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
            data.visualSprite);

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
