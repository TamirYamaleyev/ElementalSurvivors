using UnityEngine;

public enum SkillCostType
{
    MP = 0,
    HP = 1
}

[CreateAssetMenu(fileName = "SkillData", menuName = "Elemental Survivors/Combat/Skill Data")]
public class SkillDataSO : ScriptableObject
{
    [Header("Identity")]
    public string skillName = "Skill";
    public AttackType attackType = AttackType.Neutral;

    [Header("Power")]
    [Min(0)] public int basePower = 20;

    [Header("Cost")]
    public SkillCostType costType = SkillCostType.MP;
    [Min(0)] public int costValue = 5;

    [Tooltip("Player turns before this skill can be used again after a successful use.")]
    [Min(0)] public int cooldownTurns = 2;
}
