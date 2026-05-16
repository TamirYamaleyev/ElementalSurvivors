using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    [SerializeField] private EnemyTierCatalogSO catalog;
    [SerializeField] private Transform poolRoot;

    private readonly Dictionary<EnemyTier, Stack<Enemy>> pools = new();

    private void Awake()
    {
        if (poolRoot == null)
        {
            var root = new GameObject("EnemyPoolRoot");
            root.transform.SetParent(transform);
            poolRoot = root.transform;
        }

        foreach (EnemyTier tier in Enum.GetValues(typeof(EnemyTier)))
            pools[tier] = new Stack<Enemy>();
    }

    private void Start()
    {
        if (catalog?.AllEntries == null)
            return;

        foreach (var entry in catalog.AllEntries)
            PrewarmTier(entry);
    }

    private void PrewarmTier(EnemyTierCatalogSO.TierEntry entry)
    {
        if (entry.prototype == null || entry.prewarmCount <= 0)
            return;

        for (int i = 0; i < entry.prewarmCount; i++)
        {
            Enemy instance = CreateInstance(entry.prototype, entry.tier);
            ReturnToPool(instance);
        }
    }

    public Enemy Acquire(EnemyTier tier)
    {
        if (!pools.TryGetValue(tier, out Stack<Enemy> stack))
            return null;

        EnemyTierCatalogSO.TierEntry entry = catalog.GetEntry(tier);
        if (entry.prototype == null)
            return null;

        Enemy enemy = stack.Count > 0
            ? stack.Pop()
            : CreateInstance(entry.prototype, tier);

        return enemy;
    }

    public void Release(Enemy enemy)
    {
        if (enemy == null)
            return;

        ReturnToPool(enemy);
    }

    private void ReturnToPool(Enemy enemy)
    {
        enemy.OnReleased();
        enemy.transform.SetParent(poolRoot, false);

        if (pools.TryGetValue(enemy.PoolTier, out Stack<Enemy> stack))
            stack.Push(enemy);
    }

    private Enemy CreateInstance(Enemy prototype, EnemyTier tier)
    {
        Enemy instance = Instantiate(prototype, poolRoot);
        instance.BindPoolReturn(Release, tier);
        instance.OnReleased();
        return instance;
    }
}
