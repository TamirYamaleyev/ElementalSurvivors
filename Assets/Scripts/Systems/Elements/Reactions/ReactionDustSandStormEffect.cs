using UnityEngine;

public sealed class ReactionDustSandStormEffect : MonoBehaviour, IReactionGameplayEffect
{
    private ReactionEffectContext ctx;
    private ReactionGameplayDefinition def;
    private StatusPair pair;
    private EnemyRegistry registry;
    private float elapsed;
    private float tickTimer;

    public void Initialize(ReactionEffectContext context, ReactionGameplayDefinition definition)
    {
        ctx = context;
        def = definition;
        pair = context.Pair;
        registry = ReactionGameplayEffectUtility.ResolveRegistry(context.Registry);
        transform.position = ReactionGameplayEffectUtility.ResolveReactionOrigin(context);

        var burstDamage = ReactionGameplayEffectUtility.ResolveDamage(definition, context.TriggerDamage);
        if (burstDamage > 0f)
            ApplyDamageToZone(burstDamage);

        tickTimer = 0f;
        var tickDamage = def.contactDps * Mathf.Max(0.05f, def.tickInterval);
        if (tickDamage > 0f)
            ApplyDamageToZone(tickDamage);
    }

    private void Update()
    {
        if (def == null)
            return;

        registry = ReactionGameplayEffectUtility.ResolveRegistry(registry);
        if (registry == null)
            return;

        var center = ResolveCenter();
        transform.position = center;

        elapsed += Time.deltaTime;
        if (elapsed >= def.duration)
        {
            Destroy(gameObject);
            return;
        }

        tickTimer += Time.deltaTime;
        var tickInterval = Mathf.Max(0.05f, def.tickInterval);
        if (tickTimer < tickInterval)
            return;

        tickTimer -= tickInterval;
        var tickDamage = def.contactDps * tickInterval;
        if (tickDamage > 0f)
            ApplyDamageToZone(tickDamage);
    }

    private Vector3 ResolveCenter()
    {
        if (ctx.SourceEnemy != null && ctx.SourceEnemy.gameObject.activeInHierarchy)
            return ctx.SourceEnemy.transform.position + Vector3.up * 0.25f;

        return transform.position;
    }

    private void ApplyDamageToZone(float amount)
    {
        ReactionGameplayEffectUtility.ForEachEnemyInRadius(
            registry,
            ResolveCenter(),
            def.radius,
            enemy => ReactionGameplayEffectUtility.ApplyDamage(enemy, amount, pair));
    }
}
