using UnityEngine;

public sealed class ReactionCrystallizeEffect : MonoBehaviour, IReactionGameplayEffect
{
    private const float SlowMultiplier = 0.45f;

    public void Initialize(ReactionEffectContext context, ReactionGameplayDefinition definition)
    {
        var damage = ReactionGameplayEffectUtility.ResolveDamage(definition, context.TriggerDamage);
        var origin = ReactionGameplayEffectUtility.ResolveReactionOrigin(context);
        var slowDuration = Mathf.Max(0.1f, definition.duration);

        ReactionGameplayEffectUtility.ForEachEnemyInRadius(
            context.Registry,
            origin,
            definition.radius,
            enemy =>
            {
                ReactionGameplayEffectUtility.ApplyDamage(enemy, damage, context.Pair);
                enemy.AI?.ApplySlow(slowDuration, SlowMultiplier);
            });
    }
}
