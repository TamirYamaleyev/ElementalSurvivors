using UnityEngine;

public sealed class ReactionMagnetismEffect : MonoBehaviour, IReactionGameplayEffect
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
        if (tickDamage <= 0f)
            return;

        var target = ResolvePullCenter();

        ReactionGameplayEffectUtility.ForEachEnemyInRadius(
            ctx.Registry,
            target,
            def.radius,
            enemy =>
            {
                enemy.AI?.ApplyPullToward(target, def.pullSpeed, 0.2f);
                ReactionGameplayEffectUtility.ApplyDamage(enemy, tickDamage, ctx.Pair);
            });
    }

    private Vector2 ResolvePullCenter()
    {
        if (ctx.SourceEnemy != null && ctx.SourceEnemy.gameObject.activeInHierarchy)
        {
            var liveCenter = (Vector2)ctx.SourceEnemy.transform.position + Vector2.up * 0.25f;
            transform.position = liveCenter;
            return liveCenter;
        }

        transform.position = ctx.Center;
        return ctx.Center;
    }
}
