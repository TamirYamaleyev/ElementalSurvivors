using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    [System.Serializable]
    public struct TierPool
    {
        public EnemyAI prototype;
        public int prewarmCount;
    }

    [SerializeField] private TierPool levelOne;
    [SerializeField] private TierPool levelTwo;
    [SerializeField] private TierPool levelThree;
    [SerializeField] private Transform poolRoot;

    private readonly Stack<EnemyAI>[] pools = new Stack<EnemyAI>[3];

    private void Awake()
    {
        for (int i = 0; i < pools.Length; i++)
            pools[i] = new Stack<EnemyAI>();

        if (poolRoot == null)
        {
            var root = new GameObject("EnemyPoolRoot");
            root.transform.SetParent(transform);
            poolRoot = root.transform;
        }
    }

    private void Start()
    {
        Prewarm(0, levelOne);
        Prewarm(1, levelTwo);
        Prewarm(2, levelThree);
    }

    private void Prewarm(int tierIndex, TierPool tier)
    {
        if (tier.prototype == null || tier.prewarmCount <= 0)
            return;

        for (int i = 0; i < tier.prewarmCount; i++)
        {
            EnemyAI instance = CreateInstance(tier.prototype, tierIndex);
            Release(instance);
        }
    }

    public EnemyAI Acquire(int tierIndex)
    {
        if (tierIndex < 0 || tierIndex >= pools.Length)
            return null;

        EnemyAI prototype = GetPrototype(tierIndex);
        if (prototype == null)
            return null;

        EnemyAI enemy = pools[tierIndex].Count > 0
            ? pools[tierIndex].Pop()
            : CreateInstance(prototype, tierIndex);

        return enemy;
    }

    public void Release(EnemyAI enemy)
    {
        if (enemy == null)
            return;

        int tier = enemy.PoolTierIndex;
        enemy.ResetForPool();
        enemy.transform.SetParent(poolRoot, false);

        if (tier >= 0 && tier < pools.Length)
            pools[tier].Push(enemy);
    }

    private EnemyAI GetPrototype(int tierIndex)
    {
        return tierIndex switch
        {
            1 => levelTwo.prototype,
            2 => levelThree.prototype,
            _ => levelOne.prototype,
        };
    }

    private EnemyAI CreateInstance(EnemyAI prototype, int tierIndex)
    {
        EnemyAI instance = Instantiate(prototype, poolRoot);
        instance.BindPoolReturn(Release, tierIndex);
        instance.ResetForPool();
        return instance;
    }
}
