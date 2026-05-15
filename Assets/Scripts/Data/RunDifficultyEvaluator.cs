using UnityEngine;

public static class RunDifficultyEvaluator
{
    public static float GetDifficultyMultiplier(RunDifficultyProfileSO profile, float elapsedSeconds)
    {
        if (profile == null)
            return 1f;

        float interval = profile.difficultyTickIntervalSeconds;
        if (interval <= 0f)
            return 1f;

        int ticks = Mathf.FloorToInt(elapsedSeconds / interval);
        if (profile.maxDifficultyTicks > 0)
            ticks = Mathf.Min(ticks, profile.maxDifficultyTicks);

        return Mathf.Pow(profile.difficultyMultiplierPerTick, ticks);
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
