using UnityEngine;

public sealed class ReactionGrowthZoneEffect : MonoBehaviour, IReactionGameplayEffect
{
    private Vector3 zoneCenter;
    private StatusPair pair;
    private EnemyRegistry registry;
    private float duration;
    private float radius;
    private float tickInterval;
    private float tickDamage;
    private float elapsed;
    private float tickTimer;

    public void Initialize(ReactionEffectContext context, ReactionGameplayDefinition definition)
    {
        zoneCenter = ReactionGameplayEffectUtility.ResolveReactionOrigin(context);
        pair = context.Pair;
        registry = ReactionGameplayEffectUtility.ResolveRegistry(context.Registry);
        duration = Mathf.Max(0.1f, definition.duration);
        radius = definition.radius;
        tickInterval = Mathf.Max(0.05f, definition.tickInterval);
        tickDamage = definition.contactDps * tickInterval;

        transform.position = zoneCenter;

        var burstDamage = ReactionGameplayEffectUtility.ResolveDamage(definition, context.TriggerDamage);
        if (burstDamage > 0f)
            ApplyDamageToZone(burstDamage);

        tickTimer = 0f;
        ApplyDamageToZone(tickDamage);
    }

    private void Update()
    {
        if (duration <= 0f)
            return;

        registry = ReactionGameplayEffectUtility.ResolveRegistry(registry);
        if (registry == null)
            return;

        elapsed += Time.deltaTime;
        if (elapsed >= duration)
        {
            Destroy(gameObject);
            return;
        }

        tickTimer += Time.deltaTime;
        if (tickTimer < tickInterval)
            return;

        tickTimer -= tickInterval;
        ApplyDamageToZone(tickDamage);
    }

    private void ApplyDamageToZone(float amount)
    {
        if (amount <= 0f)
            return;

        ReactionGameplayEffectUtility.ForEachEnemyInRadius(
            registry,
            zoneCenter,
            radius,
            enemy => ReactionGameplayEffectUtility.ApplyDamage(enemy, amount, pair));
    }
}
