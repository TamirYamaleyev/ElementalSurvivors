using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    [SerializeField] private EnemyTierSetSO tierSet;
    [SerializeField] private Transform poolRoot;

    private TierObjectPool[] pools;

    private void Awake()
    {
        if (poolRoot == null)
        {
            var root = new GameObject("EnemyPoolRoot");
            root.transform.SetParent(transform);
            poolRoot = root.transform;
        }

        if (tierSet == null || tierSet.tiers == null)
        {
            pools = System.Array.Empty<TierObjectPool>();
            return;
        }

        pools = new TierObjectPool[tierSet.tiers.Length];
        for (int i = 0; i < tierSet.tiers.Length; i++)
        {
            Enemy prototype = tierSet.tiers[i].prototype;
            pools[i] = new TierObjectPool(prototype, poolRoot, i, BindOnCreate);
        }
    }

    private void Start()
    {
        if (tierSet == null || tierSet.tiers == null)
            return;

        for (int i = 0; i < tierSet.tiers.Length && i < pools.Length; i++)
            pools[i].Prewarm(tierSet.tiers[i].prewarmCount);
    }

    public Enemy Acquire(int tierIndex)
    {
        if (pools == null || tierIndex < 0 || tierIndex >= pools.Length)
            return null;

        return pools[tierIndex].Acquire();
    }

    public void Release(Enemy enemy)
    {
        if (enemy == null || pools == null)
            return;

        int tier = enemy.PoolTierIndex;
        if (tier >= 0 && tier < pools.Length)
            pools[tier].Release(enemy);
    }

    private void BindOnCreate(Enemy instance, int tierIndex)
    {
        instance.BindPool(Release, tierIndex);
    }
}
