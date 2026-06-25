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
        if (registry == null || action == null || radius <= 0f)
            return;

        Scratch.Clear();
        ReactionAreaVfxUtility.CollectEnemiesInRadius(registry, center, radius, Scratch, exclude);

        for (var i = 0; i < Scratch.Count; i++)
            action(Scratch[i]);
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
