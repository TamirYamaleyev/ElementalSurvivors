using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class TierObjectPool
{
    private readonly Enemy prototype;
    private readonly Transform poolRoot;
    private readonly Action<Enemy, int> onCreate;
    private readonly int tierIndex;
    private readonly int maxSize;
    private readonly Stack<Enemy> stack = new();

    public TierObjectPool(
        Enemy prototype,
        Transform poolRoot,
        int tierIndex,
        Action<Enemy, int> onCreate,
        int maxSize = 0)
    {
        this.prototype = prototype;
        this.poolRoot = poolRoot;
        this.tierIndex = tierIndex;
        this.onCreate = onCreate;
        this.maxSize = maxSize;
    }

    public void Prewarm(int count)
    {
        if (prototype == null || count <= 0)
            return;

        for (int i = 0; i < count; i++)
            Release(CreateInstance());
    }

    public Enemy Acquire()
    {
        if (prototype == null)
            return null;

        return stack.Count > 0 ? stack.Pop() : CreateInstance();
    }

    public void Release(Enemy enemy)
    {
        if (enemy == null)
            return;

        enemy.OnReleaseToPool();
        enemy.transform.SetParent(poolRoot, false);

        if (maxSize > 0 && stack.Count >= maxSize)
        {
            Object.Destroy(enemy.gameObject);
            return;
        }

        stack.Push(enemy);
    }

    private Enemy CreateInstance()
    {
        Enemy instance = Object.Instantiate(prototype, poolRoot);
        onCreate?.Invoke(instance, tierIndex);
        instance.OnReleaseToPool();
        return instance;
    }
}
