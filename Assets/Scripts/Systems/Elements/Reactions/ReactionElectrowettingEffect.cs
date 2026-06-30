using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Water + Lightning: spreads Lightning debuff to the nearest enemies in a chain (max 3 jumps).
/// </summary>
public sealed class ReactionElectrowettingEffect : MonoBehaviour, IReactionGameplayEffect
{
    private const int DefaultJumpCap = 3;
    private const float DefaultStatusDuration = 10f;

    private readonly List<Enemy> chainTargets = new();

    public IReadOnlyList<Enemy> ChainTargets => chainTargets;

    public void Initialize(ReactionEffectContext ctx, ReactionGameplayDefinition def)
    {
        chainTargets.Clear();

        var origin = ReactionGameplayEffectUtility.ResolveReactionOrigin(ctx);
        var jumpCap = def.laserCount > 0 ? def.laserCount : DefaultJumpCap;
        ReactionGameplayEffectUtility.BuildChainTargets(
            ctx.Registry,
            origin,
            jumpCap,
            def.radius,
            chainTargets,
            ctx.SourceEnemy);

        if (ctx.StatusSystem == null)
            return;

        var statusDuration = def.duration > 0f ? def.duration : DefaultStatusDuration;

        if (chainTargets.Count == 0)
        {
            if (ctx.SourceEnemy != null)
                ctx.StatusSystem.ApplySpreadStatus(ctx.SourceEnemy, StatusType.Lightning, statusDuration);
            return;
        }

        for (var i = 0; i < chainTargets.Count; i++)
            ctx.StatusSystem.ApplySpreadStatus(chainTargets[i], StatusType.Lightning, statusDuration);
    }
}
