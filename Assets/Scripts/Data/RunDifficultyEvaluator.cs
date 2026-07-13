using UnityEngine;

public static class RunDifficultyEvaluator
{
    public static float GetDifficultyMultiplier(RunDifficultyProfileSO profile, float elapsedSeconds)
    {
        return GetDifficultyMultiplier(profile, elapsedSeconds, isEndless: false);
    }

    public static float GetDifficultyMultiplier(RunDifficultyProfileSO profile, float elapsedSeconds, bool isEndless)
    {
        if (profile == null)
            return 1f;

        float interval = profile.difficultyTickIntervalSeconds;
        float baseMultiplier = 1f;
        if (interval > 0f)
        {
            int ticks = Mathf.FloorToInt(elapsedSeconds / interval);
            if (profile.maxDifficultyTicks > 0)
                ticks = Mathf.Min(ticks, profile.maxDifficultyTicks);

            baseMultiplier = Mathf.Pow(profile.difficultyMultiplierPerTick, ticks);
        }

        if (!isEndless || elapsedSeconds <= profile.sessionDurationSeconds)
            return baseMultiplier;

        float extraMinutes = (elapsedSeconds - profile.sessionDurationSeconds) / 60f;
        float extraPerMinute = profile.endlessExtraMultiplierPerMinute;
        if (extraPerMinute <= 0f)
            return baseMultiplier;

        return baseMultiplier * (1f + extraPerMinute * extraMinutes);
    }

    public static int GetPrefabIndex(RunDifficultyProfileSO profile, float elapsedSeconds)
    {
        if (profile == null)
            return 0;

        if (elapsedSeconds < profile.tier2StartSeconds)
            return 0;

        if (elapsedSeconds < profile.tier3StartSeconds)
            return 1;

        return 2;
    }

    [System.Obsolete("Boss schedule is milestone-based in EnemySpawner (tier2/tier3/session times).")]
    public static bool ShouldSpawnBoss(RunDifficultyProfileSO profile, float elapsedSeconds, ref float nextBossAt)
    {
        if (profile == null)
            return false;

        if (profile.bossSpawnIntervalSeconds <= 0f)
            return false;

        if (elapsedSeconds < nextBossAt)
            return false;

        nextBossAt += profile.bossSpawnIntervalSeconds;
        return true;
    }
}
