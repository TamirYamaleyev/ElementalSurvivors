using System.Collections.Generic;
using UnityEngine;

public static class ReactionGameplayEffectUtility
{
    private static readonly List<Enemy> Scratch = new();

    public static void ForEachEnemyInRadius(
        EnemyRegistry registry,
        Vector3 center,
        float radius,
        System.Action<Enemy> action,
        Enemy exclude = null)
    {
        if (action == null || radius <= 0f)
            return;

        registry = ResolveRegistry(registry);
        if (registry == null)
            return;

        Scratch.Clear();
        ReactionAreaVfxUtility.CollectEnemiesInRadius(registry, center, radius, Scratch, exclude);

        for (var i = 0; i < Scratch.Count; i++)
            action(Scratch[i]);
    }

    public static EnemyRegistry ResolveRegistry(EnemyRegistry registry)
    {
        if (registry != null)
            return registry;

        return Object.FindAnyObjectByType<EnemyRegistry>();
    }

    public static float ResolveDamage(ReactionGameplayDefinition def, float triggerDamage)
    {
        if (def == null)
            return 0f;

        var scaled = triggerDamage > 0f
            ? triggerDamage * def.damageMultiplier
            : def.flatDamage;

        return Mathf.Max(0f, scaled);
    }

    public static void ApplyDamage(Enemy enemy, float amount, StatusPair pair)
    {
        if (enemy == null || amount <= 0f)
            return;

        var color = ElementStatusPalette.GetDamageNumberColor(pair.First);
        enemy.GetComponent<EnemyHealth>()?.TakeDamage(amount, color);
    }

    public static void ApplyRadialKnockback(Enemy enemy, Vector2 origin, float impulse)
    {
        if (enemy == null || impulse <= 0f)
            return;

        var dir = (Vector2)enemy.transform.position - origin;
        if (dir.sqrMagnitude < 1e-6f)
            dir = Random.insideUnitCircle.normalized;

        enemy.AI?.ApplyKnockback(dir, impulse);
    }

    public static Vector2 ResolveReactionOrigin(ReactionEffectContext ctx)
    {
        if (ctx.SourceEnemy != null && ctx.SourceEnemy.gameObject.activeInHierarchy)
            return (Vector2)ctx.SourceEnemy.transform.position + Vector2.up * 0.25f;

        return ctx.Center;
    }

    public static void BuildChainTargets(
        EnemyRegistry registry,
        Vector2 startPosition,
        int chainCount,
        float range,
        List<Enemy> result,
        Enemy exclude = null)
    {
        result.Clear();
        registry = ResolveRegistry(registry);
        if (registry == null || chainCount <= 0 || range <= 0f)
            return;

        var visited = new HashSet<Enemy>();
        if (exclude != null)
            visited.Add(exclude);

        var currentPos = startPosition;
        var maxSqr = range * range;

        for (var i = 0; i < chainCount; i++)
        {
            Enemy best = null;
            var bestDist = float.MaxValue;

            foreach (var enemy in registry.ActiveEnemies)
            {
                if (enemy == null || visited.Contains(enemy) || !enemy.gameObject.activeInHierarchy)
                    continue;

                var delta = (Vector2)enemy.transform.position - currentPos;
                var distSqr = delta.sqrMagnitude;
                if (distSqr > maxSqr || distSqr >= bestDist)
                    continue;

                bestDist = distSqr;
                best = enemy;
            }

            if (best == null)
                break;

            result.Add(best);
            visited.Add(best);
            currentPos = best.transform.position;
        }
    }

    public static void PlayChildVfx(ReactionEffectContext ctx, GameObject vfxPrefab)
    {
        if (vfxPrefab == null)
            return;

        var vfxCtx = new ReactionVfxContext(ctx.Center, ctx.SourceEnemy, ctx.Registry);
        var instance = Object.Instantiate(vfxPrefab);
        instance.transform.position = ctx.Center;

        foreach (var behaviour in instance.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (behaviour is IReactionWorldVfx worldVfx)
                worldVfx.Initialize(vfxCtx);
        }

        foreach (var ps in instance.GetComponentsInChildren<ParticleSystem>(true))
        {
            ps.Clear(true);
            ps.Play(true);
        }
    }
}
