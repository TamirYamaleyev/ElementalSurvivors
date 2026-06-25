using UnityEngine;

public sealed class ReactionExplosionEffect : MonoBehaviour, IReactionGameplayEffect
{
    private ReactionEffectContext ctx;
    private ReactionGameplayDefinition def;

    public void Initialize(ReactionEffectContext context, ReactionGameplayDefinition definition)
    {
        ctx = context;
        def = definition;

        var damage = ReactionGameplayEffectUtility.ResolveDamage(def, ctx.TriggerDamage);
        var center = ctx.Center;

        ReactionGameplayEffectUtility.ForEachEnemyInRadius(
            ctx.Registry,
            center,
            def.radius,
            enemy =>
            {
                ReactionGameplayEffectUtility.ApplyDamage(enemy, damage, ctx.Pair);

                var knockDir = (Vector2)(enemy.transform.position - center);
                if (knockDir.sqrMagnitude < 1e-6f)
                    knockDir = Random.insideUnitCircle.normalized;

                enemy.AI?.ApplyKnockback(knockDir, def.knockbackImpulse);
            });
    }
}
