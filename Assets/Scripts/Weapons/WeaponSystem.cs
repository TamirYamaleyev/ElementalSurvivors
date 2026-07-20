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

    [SerializeField] private int maxWeapons = 4;

    private readonly List<WeaponInstance> runtimeWeapons = new();
    private WeaponSystemContext context;

    public event Action OnWeaponsChanged;

    public IReadOnlyList<WeaponInstance> Weapons => runtimeWeapons;
    public IReadOnlyList<WeaponLoadoutEntry> AvailableWeapons => weapons;

    private void NotifyWeaponsChanged()
    {
        OnWeaponsChanged?.Invoke();
    }

    private void BindWeaponEvents(WeaponInstance weapon)
    {
        weapon.OnLevelUp += () =>
        {
            if (weapon.definition.behaviorType == WeaponBehaviorType.Orbit)
            {
                context.OrbitSystem.ClearOrbitObjects();
            }

            NotifyWeaponsChanged();
        };
    }

    public bool CanAddWeapon()
    {
        return runtimeWeapons.Count < maxWeapons;
    }

    public bool HasWeapon(WeaponDefinition definition)
    {
        foreach (var weapon in runtimeWeapons)
        {
            if (weapon.definition == definition) 
                return true;
        }

        return false;
    }

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

            var weapon = new WeaponInstance(entry.definition, entry.level);

            BindWeaponEvents(weapon);
            runtimeWeapons.Add(weapon);
        }
    }

    public WeaponInstance UnlockWeapon(WeaponDefinition definition)
    {
        if (!CanAddWeapon())
            return null;

        if (HasWeapon(definition))
            return null;

        var weapon = new WeaponInstance(definition, 1);

        BindWeaponEvents(weapon);
        runtimeWeapons.Add(weapon);

        NotifyWeaponsChanged();

        return weapon;
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
