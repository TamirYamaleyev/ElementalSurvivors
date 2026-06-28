using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    public const int BossPoolTierIndex = -2;

    [SerializeField] private EnemyTierSetSO tierSet;
    [SerializeField] private Transform poolRoot;
    [SerializeField] private int bossPrewarmCount = 2;

    private TierObjectPool[] pools;
    private TierObjectPool bossPool;

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

        var bossPrototype = tierSet.GetBossPrototype();
        if (bossPrototype != null)
        {
            bossPool = new TierObjectPool(
                bossPrototype,
                poolRoot,
                BossPoolTierIndex,
                BindBossOnCreate);
        }
    }

    private void Start()
    {
        if (tierSet == null || tierSet.tiers == null)
            return;

        for (int i = 0; i < tierSet.tiers.Length && i < pools.Length; i++)
            pools[i].Prewarm(tierSet.tiers[i].prewarmCount);

        bossPool?.Prewarm(bossPrewarmCount);
    }

    public Enemy Acquire(int tierIndex)
    {
        if (pools == null || tierIndex < 0 || tierIndex >= pools.Length)
            return null;

        return pools[tierIndex].Acquire();
    }

    public Enemy AcquireBoss()
    {
        if (bossPool != null)
            return bossPool.Acquire();

        if (pools == null || pools.Length == 0)
            return null;

        return pools[pools.Length - 1].Acquire();
    }

    public void Release(Enemy enemy)
    {
        if (enemy == null)
            return;

        if (enemy.PoolTierIndex == BossPoolTierIndex)
        {
            bossPool?.Release(enemy);
            return;
        }

        if (pools == null)
            return;

        int tier = enemy.PoolTierIndex;
        if (tier >= 0 && tier < pools.Length)
            pools[tier].Release(enemy);
    }

    private void BindOnCreate(Enemy instance, int tierIndex)
    {
        instance.BindPool(Release, tierIndex);
    }

    private void BindBossOnCreate(Enemy instance, int tierIndex)
    {
        instance.BindPool(Release, BossPoolTierIndex);
    }
}
