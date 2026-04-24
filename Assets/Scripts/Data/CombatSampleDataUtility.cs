#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class CombatSampleDataUtility
{
    [MenuItem("Elemental Survivors/Create Combat Prototype Sample Data")]
    public static void CreateSampleData()
    {
        const string dataFolder = "Assets/Data/Combat";
        EnsureFolder("Assets", "Data");
        EnsureFolder("Assets/Data", "Combat");

        CreateStatsAsset($"{dataFolder}/Player_CombatStats.asset", 120, 40, 14, 8, 110);
        CreateStatsAsset($"{dataFolder}/Enemy_Goblin_CombatStats.asset", 90, 20, 11, 6, 95);
        CreateSkillAsset($"{dataFolder}/Skill_AxeSwing.asset", "Axe Swing", AttackType.Jazz, 16, SkillCostType.MP, 6, 2);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Combat sample assets created under Assets/Data/Combat.");
    }

    private static void CreateStatsAsset(string path, int hp, int mp, int atk, int def, int spd)
    {
        CombatStatsSO stats = AssetDatabase.LoadAssetAtPath<CombatStatsSO>(path);
        if (stats == null)
        {
            stats = ScriptableObject.CreateInstance<CombatStatsSO>();
            AssetDatabase.CreateAsset(stats, path);
        }

        stats.maxHP = hp;
        stats.maxMP = mp;
        stats.attack = atk;
        stats.defense = def;
        stats.speed = spd;
        EditorUtility.SetDirty(stats);
    }

    private static void CreateSkillAsset(string path, string skillName, AttackType type, int basePower, SkillCostType costType, int costValue, int cooldownTurns)
    {
        SkillDataSO skill = AssetDatabase.LoadAssetAtPath<SkillDataSO>(path);
        if (skill == null)
        {
            skill = ScriptableObject.CreateInstance<SkillDataSO>();
            AssetDatabase.CreateAsset(skill, path);
        }

        skill.skillName = skillName;
        skill.attackType = type;
        skill.basePower = basePower;
        skill.costType = costType;
        skill.costValue = costValue;
        skill.cooldownTurns = cooldownTurns;
        EditorUtility.SetDirty(skill);
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = $"{parent}/{child}";
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(parent, child);
    }
}
#endif
