using UnityEngine;

public class CombatEntity
{
    private const float ActionValueBase = 10000f;

    private readonly int combatMaxHP;

    public CombatEntity(MonoBehaviour owner, CombatStatsSO statsData, bool isPlayer, string displayName, int? currentHpOverride = null, int? maxHpOverride = null)
    {
        Owner = owner;
        StatsData = statsData;
        IsPlayer = isPlayer;
        DisplayName = displayName;

        int defaultMax = statsData != null ? statsData.maxHP : 1;
        combatMaxHP = Mathf.Max(1, maxHpOverride ?? defaultMax);
        int startHp = currentHpOverride ?? combatMaxHP;
        CurrentHP = Mathf.Clamp(startHp, 0, combatMaxHP);
        CurrentMP = statsData != null ? statsData.maxMP : 0;
        RecalculateTurnActionValue();
        CurrentActionValue = TurnActionValue;
    }

    public MonoBehaviour Owner { get; }
    public CombatStatsSO StatsData { get; }
    public bool IsPlayer { get; }
    public string DisplayName { get; }

    public int CurrentHP { get; private set; }
    public int CurrentMP { get; private set; }
    public float CurrentActionValue { get; private set; }
    public float TurnActionValue { get; private set; }
    public bool Defending { get; private set; }

    public int Attack => StatsData != null ? StatsData.attack : 1;
    public int Defense => StatsData != null ? StatsData.defense : 0;
    public int Speed => StatsData != null ? Mathf.Max(1, StatsData.speed) : 1;
    public int MaxHP => combatMaxHP;
    public int MaxMP => StatsData != null ? StatsData.maxMP : 0;

    public bool IsDead => CurrentHP <= 0;

    public void ReduceActionValue(float amount)
    {
        CurrentActionValue = Mathf.Max(0f, CurrentActionValue - amount);
    }

    public void ResetActionValue()
    {
        RecalculateTurnActionValue();
        CurrentActionValue = TurnActionValue;
    }

    public void SetDefending(bool active)
    {
        Defending = active;
    }

    public void ConsumeMP(int amount)
    {
        CurrentMP = Mathf.Clamp(CurrentMP - Mathf.Max(0, amount), 0, MaxMP);
    }

    public void ConsumeHP(int amount)
    {
        CurrentHP = Mathf.Clamp(CurrentHP - Mathf.Max(0, amount), 0, MaxHP);
    }

    public void Heal(int amount)
    {
        CurrentHP = Mathf.Clamp(CurrentHP + Mathf.Max(0, amount), 0, MaxHP);
    }

    public void TakeDamage(int amount)
    {
        CurrentHP = Mathf.Clamp(CurrentHP - Mathf.Max(0, amount), 0, MaxHP);
    }

    public void RecalculateTurnActionValue()
    {
        TurnActionValue = ActionValueBase / Speed;
    }
}
