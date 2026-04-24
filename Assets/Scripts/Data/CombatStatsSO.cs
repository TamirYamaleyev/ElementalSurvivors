using UnityEngine;

[CreateAssetMenu(fileName = "CombatStats", menuName = "Elemental Survivors/Combat/Combat Stats")]
public class CombatStatsSO : ScriptableObject
{
    [Header("Core Stats")]
    [Min(1)] public int maxHP = 100;
    [Min(0)] public int maxMP = 30;
    [Min(1)] public int attack = 10;
    [Min(0)] public int defense = 5;
    [Min(1)] public int speed = 100;
}
