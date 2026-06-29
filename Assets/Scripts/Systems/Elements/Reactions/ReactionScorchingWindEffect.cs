using System.Collections.Generic;
using UnityEngine;

public sealed class ReactionScorchingWindEffect : MonoBehaviour, IReactionGameplayEffect
{
    private static readonly List<Enemy> HitScratch = new();

    public void Initialize(ReactionEffectContext ctx, ReactionGameplayDefinition def)
    {
        var origin = ctx.PlayerTransform != null
            ? (Vector2)ctx.PlayerTransform.position
            : (Vector2)ctx.Center;

        var damage = ReactionGameplayEffectUtility.ResolveDamage(def, ctx.TriggerDamage);
        var hit = new HashSet<Enemy>();

        for (var i = 0; i < def.laserCount; i++)
        {
            var angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            CollectHitsAlongBeam(ctx.Registry, origin, dir, def.laserLength, def.laserHalfWidth, HitScratch);

            for (var j = 0; j < HitScratch.Count; j++)
            {
                var enemy = HitScratch[j];
                if (enemy == null || !hit.Add(enemy))
                    continue;

                ReactionGameplayEffectUtility.ApplyDamage(enemy, damage, ctx.Pair);
            }
        }
    }

    private static void CollectHitsAlongBeam(
        EnemyRegistry registry,
        Vector2 origin,
        Vector2 direction,
        float length,
        float halfWidth,
        List<Enemy> results)
    {
        results.Clear();
        if (registry == null || length <= 0f)
            return;

        var dir = direction.sqrMagnitude > 1e-6f ? direction.normalized : Vector2.up;
        var widthSq = halfWidth * halfWidth;

        foreach (var enemy in registry.ActiveEnemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
                continue;

            var point = (Vector2)enemy.transform.position;
            var along = Vector2.Dot(point - origin, dir);
            if (along < 0f || along > length)
                continue;

            var closest = origin + dir * along;
            if ((point - closest).sqrMagnitude <= widthSq)
                results.Add(enemy);
        }
    }
}
