using UnityEngine;

public static class ActionResolver
{
    public static CombatResult ResolveDamageAction(CombatEntity attacker, CombatEntity target, int basePower, AttackType attackType, BGMType bgmType)
    {
        float multiplier = TypeMatchCalculator.Calculate(attackType, bgmType);
        int attackValue = Mathf.Max(1, attacker.Attack + basePower);
        int defenseValue = target.Defense;

        if (target.Defending)
            defenseValue = Mathf.RoundToInt(defenseValue * 1.5f);

        int rawDamage = Mathf.Max(1, attackValue - defenseValue);
        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(rawDamage * multiplier));

        target.TakeDamage(finalDamage);
        target.SetDefending(false);

        return new CombatResult
        {
            DamageDealt = finalDamage,
            TypeMultiplier = multiplier
        };
    }

    public static CombatResult ResolveDefend(CombatEntity actor)
    {
        actor.SetDefending(true);
        return new CombatResult { DefendApplied = true };
    }

    public static CombatResult ResolveRun(CombatEntity runner, CombatEntity opponent)
    {
        float hpRatio = runner.MaxHP <= 0 ? 0f : (float)runner.CurrentHP / runner.MaxHP;
        float chance = (runner.Speed - opponent.Speed + (hpRatio * 100f)) / 2f;
        chance = Mathf.Clamp(chance, 10f, 90f);
        bool success = Random.Range(0f, 100f) <= chance;

        return new CombatResult { RunSucceeded = success };
    }
}
