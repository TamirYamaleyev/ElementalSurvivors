using UnityEngine;

public sealed class ReactionHailEffect : MonoBehaviour, IReactionGameplayEffect
{
    private ReactionEffectContext ctx;
    private ReactionGameplayDefinition def;
    private float elapsed;
    private bool stunApplied;

    public void Initialize(ReactionEffectContext context, ReactionGameplayDefinition definition)
    {
        ctx = context;
        def = definition;
        transform.position = ctx.Center;
        ApplyHailStun();
    }

    private void Update()
    {
        if (def == null)
            return;

        elapsed += Time.deltaTime;
        if (elapsed >= def.duration)
            Destroy(gameObject);
    }

    private void ApplyHailStun()
    {
        if (stunApplied)
            return;

        stunApplied = true;

        var damage = ReactionGameplayEffectUtility.ResolveDamage(def, ctx.TriggerDamage);

        ReactionGameplayEffectUtility.ForEachEnemyInRadius(
            ctx.Registry,
            ctx.Center,
            def.radius,
            enemy =>
            {
                if (damage > 0f)
                    ReactionGameplayEffectUtility.ApplyDamage(enemy, damage, ctx.Pair);

                var ai = enemy.AI;
                if (ai == null)
                    return;

                if (ai.CanBeStunnedByHail())
                    ai.ApplyStun(def.stunDuration);

                ai.AddHailStunImmunity(def.hailImmunityGain);
            });
    }
}
