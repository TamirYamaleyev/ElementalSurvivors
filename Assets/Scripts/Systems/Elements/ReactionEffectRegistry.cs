using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ReactionEffectEntry
{
    public Func<GameObject, IReactionGameplayEffect> CreateEffect;
    public bool ForceSustainedLifecycle;
    public float InstantCleanupDelay = 0.2f;
}

public static class ReactionEffectRegistry
{
    private static readonly Dictionary<StatusPair, ReactionEffectEntry> Entries = new();

    static ReactionEffectRegistry()
    {
        Register(StatusType.Fire, StatusType.Water, root => root.AddComponent<ReactionVaporizeZoneEffect>(), forceSustained: true);
        Register(StatusType.Fire, StatusType.Earth, root => root.AddComponent<ReactionCrystallizeEffect>(), cleanup: 0.35f);
        Register(StatusType.Fire, StatusType.Wind, root => root.AddComponent<ReactionScorchingWindEffect>(), cleanup: 0.4f);
        Register(StatusType.Fire, StatusType.Lightning, root => root.AddComponent<ReactionExplosionEffect>(), cleanup: 0.9f);
        Register(StatusType.Water, StatusType.Wind, root => root.AddComponent<ReactionHailEffect>(), forceSustained: true);
        Register(StatusType.Water, StatusType.Earth, root => root.AddComponent<ReactionGrowthZoneEffect>(), forceSustained: true);
        Register(StatusType.Water, StatusType.Lightning, root => root.AddComponent<ReactionElectrowettingEffect>(), cleanup: 0.25f);
        Register(StatusType.Wind, StatusType.Earth, root => root.AddComponent<ReactionDustSandStormEffect>(), forceSustained: true);
        Register(StatusType.Wind, StatusType.Lightning, root => root.AddComponent<ReactionMagnetismEffect>(), forceSustained: true);
        Register(StatusType.Earth, StatusType.Lightning, root => root.AddComponent<ReactionStaticChargeEffect>(), cleanup: 1.8f);
    }

    private static void Register(
        StatusType a,
        StatusType b,
        Func<GameObject, IReactionGameplayEffect> factory,
        bool forceSustained = false,
        float cleanup = 0.2f)
    {
        var pair = new StatusPair(a, b);
        Entries[pair] = new ReactionEffectEntry
        {
            CreateEffect = factory,
            ForceSustainedLifecycle = forceSustained,
            InstantCleanupDelay = cleanup
        };
    }

    public static bool TryGet(StatusPair pair, out ReactionEffectEntry entry)
        => Entries.TryGetValue(pair, out entry);

    public static bool UsesSustainedLifecycle(StatusPair pair, ReactionGameplayDefinition definition)
    {
        if (definition != null && definition.mode == ReactionGameplayMode.Sustained)
            return true;

        return TryGet(pair, out var entry) && entry.ForceSustainedLifecycle;
    }

    public static float GetInstantCleanupDelay(StatusPair pair)
        => TryGet(pair, out var entry) ? entry.InstantCleanupDelay : 0.2f;

    public static IReactionGameplayEffect CreateGameplayComponent(GameObject root, StatusPair pair)
        => TryGet(pair, out var entry) ? entry.CreateEffect(root) : null;
}
