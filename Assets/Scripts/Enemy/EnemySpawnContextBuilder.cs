using UnityEngine;

public static class EnemySpawnContextBuilder
{
    public static EnemySpawnContext Build(
        RunDifficultyProfileSO profile,
        EnemyTierCatalogSO catalog,
        float elapsedSeconds,
        EnemyTier tier,
        bool isBoss)
    {
        float multiplier = RunDifficultyEvaluator.GetDifficultyMultiplier(profile, elapsedSeconds);
        if (isBoss)
            multiplier *= profile.bossExtraStatMultiplier;

        EnemyTierCatalogSO.TierEntry entry = catalog.GetEntry(tier);
        EnemyStatsSO baseline = entry.baseline;

        float baseHp = baseline != null ? baseline.maxHealth : 1f;
        float baseDamage = baseline != null ? baseline.contactDamage : 1f;

        float bossScale = isBoss ? profile.bossVisualScale : 1f;

        return new EnemySpawnContext(
            baseHp * multiplier,
            baseDamage * multiplier,
            isBoss,
            bossScale);
    }
}
