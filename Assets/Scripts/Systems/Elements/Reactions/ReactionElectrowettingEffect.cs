using UnityEngine;

public sealed class ReactionElectrowettingEffect : MonoBehaviour, IReactionGameplayEffect
{
    public void Initialize(ReactionEffectContext ctx, ReactionGameplayDefinition def)
    {
        var damage = ReactionGameplayEffectUtility.ResolveDamage(def, ctx.TriggerDamage);
        if (damage <= 0f)
            return;

        ReactionGameplayEffectUtility.ForEachEnemyInRadius(
            ctx.Registry,
            ctx.Center,
            def.radius,
            enemy => ReactionGameplayEffectUtility.ApplyDamage(enemy, damage, ctx.Pair),
            ctx.SourceEnemy);

    }
}
