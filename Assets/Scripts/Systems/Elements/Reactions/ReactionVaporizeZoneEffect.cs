using UnityEngine;

public sealed class ReactionVaporizeZoneEffect : MonoBehaviour, IReactionGameplayEffect
{
    private ReactionEffectContext ctx;
    private ReactionGameplayDefinition def;
    private float elapsed;
    private float tickTimer;

    public void Initialize(ReactionEffectContext context, ReactionGameplayDefinition definition)
    {
        ctx = context;
        def = definition;
        transform.position = ctx.Center;
    }

    private void Update()
    {
        if (def == null)
            return;

        elapsed += Time.deltaTime;
        if (elapsed >= def.duration)
        {
            Destroy(gameObject);
            return;
        }

        tickTimer += Time.deltaTime;
        if (tickTimer < def.tickInterval)
            return;

        tickTimer = 0f;
        var tickDamage = def.contactDps * def.tickInterval;

        ReactionGameplayEffectUtility.ForEachEnemyInRadius(
            ctx.Registry,
            transform.position,
            def.radius,
            enemy => ReactionGameplayEffectUtility.ApplyDamage(enemy, tickDamage, ctx.Pair));
    }
}
