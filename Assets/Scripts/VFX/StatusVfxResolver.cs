using System.Collections.Generic;

/// <summary>Desired elemental status / reaction visuals derived from active unique statuses.</summary>
public readonly struct StatusVfxPlan
{
    public readonly StatusType? SoloElement;
    public readonly StatusPair[] ReactionPairs;

    public StatusVfxPlan(StatusType? soloElement, StatusPair[] reactionPairs)
    {
        SoloElement = soloElement;
        ReactionPairs = reactionPairs ?? System.Array.Empty<StatusPair>();
    }

    public static readonly StatusVfxPlan Empty = new(null, System.Array.Empty<StatusPair>());
}

/// <summary>
/// Pure resolver: 1 unique status → base VFX; 2+ → all catalog reaction pairs among active types.
/// </summary>
public static class StatusVfxResolver
{
    public static StatusVfxPlan Build(IReadOnlyList<StatusType> uniqueActive, ReactionVfxCatalogSO catalog)
    {
        if (uniqueActive == null || uniqueActive.Count == 0)
            return StatusVfxPlan.Empty;

        if (uniqueActive.Count == 1)
            return new StatusVfxPlan(uniqueActive[0], System.Array.Empty<StatusPair>());

        if (catalog == null)
            return StatusVfxPlan.Empty;

        var pairs = new List<StatusPair>();
        for (var i = 0; i < uniqueActive.Count; i++)
        {
            if (uniqueActive[i] == StatusType.None)
                continue;

            for (var j = i + 1; j < uniqueActive.Count; j++)
            {
                if (uniqueActive[j] == StatusType.None)
                    continue;

                var pair = new StatusPair(uniqueActive[i], uniqueActive[j]);
                if (!pair.IsValid)
                    continue;

                if (catalog.GetPrefab(pair.First, pair.Second) != null)
                    pairs.Add(pair);
            }
        }

        return new StatusVfxPlan(null, pairs.ToArray());
    }
}
