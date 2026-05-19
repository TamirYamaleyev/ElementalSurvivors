using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerStats : MonoBehaviour, IStatModifierTarget
{
    [SerializeField] private PlayerBaseStatsSO baseStats;

    private readonly List<PlayerStatModifier> _modifiers = new();
    private PlayerStatsSnapshot _current;

    public PlayerStatsSnapshot Current => _current;

    public event Action<PlayerStatsSnapshot> OnStatsChanged;

    void Awake()
    {
        Recalculate();
    }

    public void ApplyModifier(PlayerStatModifier modifier)
    {
        _modifiers.Add(modifier);
        Recalculate();
    }

    public void RemoveModifier(PlayerStatModifier modifier)
    {
        _modifiers.Remove(modifier);
        Recalculate();
    }

    public void ClearModifiers()
    {
        _modifiers.Clear();
        Recalculate();
    }

    private void Recalculate()
    {
        _current = StatCalculator.Compute(baseStats, _modifiers);
        OnStatsChanged?.Invoke(_current);
    }
}
