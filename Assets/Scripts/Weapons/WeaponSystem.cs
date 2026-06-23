using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct WeaponLoadoutEntry
{
    public WeaponDefinition definition;
    public int level;
}

public class WeaponSystem : MonoBehaviour
{
    [SerializeField] private List<WeaponLoadoutEntry> weapons = new();

    private readonly List<WeaponInstance> runtimeWeapons = new();
    private WeaponSystemContext context;

    public void Initialize(WeaponSystemContext ctx)
    {
        context = ctx;

        if (ctx.Targeting != null && ctx.EnemyRegistry != null)
            ctx.Targeting.Initialize(ctx.EnemyRegistry);

        runtimeWeapons.Clear();
        if (weapons == null)
            return;

        foreach (var entry in weapons)
        {
            if (entry.definition == null || entry.level == 0)
                continue;

            runtimeWeapons.Add(new WeaponInstance(entry.definition, entry.level));
        }
    }

    private void Update()
    {
        if (context?.Targeting == null || runtimeWeapons.Count == 0)
            return;

        foreach (var w in runtimeWeapons)
        {
            if (w.level == 0)
                continue;

            var target = context.Targeting.GetNearest(transform.position, w.Current.range);
            w.Tick(Time.deltaTime, target, context);
        }
    }
}
