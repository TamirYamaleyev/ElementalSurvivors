using UnityEngine;

public sealed class ReactionStaticChargeEffect : MonoBehaviour, IReactionGameplayEffect
{
    public void Initialize(ReactionEffectContext context, ReactionGameplayDefinition definition)
    {
        var damage = ReactionGameplayEffectUtility.ResolveDamage(definition, context.TriggerDamage);
        var origin = ReactionGameplayEffectUtility.ResolveReactionOrigin(context);
        var stunDuration = Mathf.Max(0f, definition.stunDuration);

        ReactionGameplayEffectUtility.ForEachEnemyInRadius(
            context.Registry,
            origin,
            definition.radius,
            enemy =>
            {
                if (damage > 0f)
                    ReactionGameplayEffectUtility.ApplyDamage(enemy, damage, context.Pair);

                var ai = enemy.AI;
                if (ai == null || stunDuration <= 0f)
                    return;

                if (ai.CanBeStunnedByHail())
                    ai.ApplyStun(stunDuration);

                ai.AddHailStunImmunity(definition.hailImmunityGain);
            });
    }
}
