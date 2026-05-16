using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyTierCatalog", menuName = "Elemental Survivors/Enemy/Tier Catalog")]
public class EnemyTierCatalogSO : ScriptableObject
{
    [Serializable]
    public struct TierEntry
    {
        public EnemyTier tier;
        public Enemy prototype;
        public EnemyStatsSO baseline;
        public int prewarmCount;
    }

    [SerializeField] private TierEntry[] tiers;

    public TierEntry GetEntry(EnemyTier tier)
    {
        if (tiers == null)
            return default;

        for (int i = 0; i < tiers.Length; i++)
        {
            if (tiers[i].tier == tier)
                return tiers[i];
        }

        return default;
    }

    public TierEntry[] AllEntries => tiers;
}
