using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyTierSet", menuName = "Elemental Survivors/Enemy Tier Set")]
public class EnemyTierSetSO : ScriptableObject
{
    [Serializable]
    public struct TierEntry
    {
        public Enemy prototype;
        public int prewarmCount;
    }

    public TierEntry[] tiers;
    public Enemy bossPrototype;

    public Enemy GetTierPrototype(int tierIndex)
    {
        if (tiers == null || tiers.Length == 0)
            return null;

        tierIndex = Mathf.Clamp(tierIndex, 0, tiers.Length - 1);
        return tiers[tierIndex].prototype;
    }

    public Enemy GetBossPrototype()
    {
        if (bossPrototype != null)
            return bossPrototype;

        if (tiers == null || tiers.Length == 0)
            return null;

        return tiers[tiers.Length - 1].prototype;
    }
}
