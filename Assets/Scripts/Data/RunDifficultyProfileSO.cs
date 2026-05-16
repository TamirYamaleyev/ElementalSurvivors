using UnityEngine;

[CreateAssetMenu(fileName = "RunDifficultyProfile", menuName = "Elemental Survivors/Run Difficulty Profile")]
public class RunDifficultyProfileSO : ScriptableObject
{
    [Header("Session")]
    public float sessionDurationSeconds = 600f;
    public bool stopSpawningWhenSessionEnds = true;

    [Header("Difficulty scaling")]
    public float difficultyTickIntervalSeconds = 60f;
    public float difficultyMultiplierPerTick = 1.1f;
    [Tooltip("0 = no cap except session length.")]
    public int maxDifficultyTicks = 0;

    [Header("Enemy tiers")]
    public float tier2StartSeconds = 200f;
    public float tier3StartSeconds = 400f;

    [Header("Boss")]
    public float firstBossDelaySeconds = 198f;
    public float bossSpawnIntervalSeconds = 198f;
    public float bossExtraStatMultiplier = 10f;
    public float bossVisualScale = 1.5f;
}
