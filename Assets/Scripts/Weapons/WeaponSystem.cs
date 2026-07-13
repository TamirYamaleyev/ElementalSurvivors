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

    public int GetLevel(WeaponDefinition def)
    {
        if (def == null || weapons == null)
            return 0;

        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i].definition == def)
                return Mathf.Max(0, weapons[i].level);
        }

        return 0;
    }

    public bool IsMaxed(WeaponDefinition def)
    {
        if (def == null || def.MaxLevel <= 0)
            return true;

        return GetLevel(def) >= def.MaxLevel;
    }

    public bool TryLevelUp(WeaponDefinition def)
    {
        if (def == null || def.MaxLevel <= 0 || weapons == null)
            return false;

        int index = FindLoadoutIndex(def);
        if (index < 0)
        {
            weapons.Add(new WeaponLoadoutEntry { definition = def, level = 0 });
            index = weapons.Count - 1;
        }

        var entry = weapons[index];
        int current = Mathf.Max(0, entry.level);
        if (current >= def.MaxLevel)
            return false;

        int next = current + 1;
        entry.level = next;
        weapons[index] = entry;

        var runtime = FindRuntime(def);
        if (runtime == null)
        {
            runtimeWeapons.Add(new WeaponInstance(def, next));
            return true;
        }

        return runtime.TryLevelUp(def.MaxLevel);
    }

    private int FindLoadoutIndex(WeaponDefinition def)
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i].definition == def)
                return i;
        }

        return -1;
    }

    private WeaponInstance FindRuntime(WeaponDefinition def)
    {
        for (int i = 0; i < runtimeWeapons.Count; i++)
        {
            if (runtimeWeapons[i].definition == def)
                return runtimeWeapons[i];
        }

        return null;
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
