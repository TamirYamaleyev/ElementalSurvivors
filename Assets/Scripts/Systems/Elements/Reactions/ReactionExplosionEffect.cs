using UnityEngine;

public sealed class ReactionExplosionEffect : MonoBehaviour, IReactionGameplayEffect
{
    public void Initialize(ReactionEffectContext context, ReactionGameplayDefinition definition)
    {
        var damage = ReactionGameplayEffectUtility.ResolveDamage(definition, context.TriggerDamage);
        var origin = ReactionGameplayEffectUtility.ResolveReactionOrigin(context);

        ReactionGameplayEffectUtility.ForEachEnemyInRadius(
            context.Registry,
            origin,
            definition.radius,
            enemy =>
            {
                ReactionGameplayEffectUtility.ApplyDamage(enemy, damage, context.Pair);
                ReactionGameplayEffectUtility.ApplyRadialKnockback(
                    enemy,
                    origin,
                    definition.knockbackImpulse);
            });
    }
}
