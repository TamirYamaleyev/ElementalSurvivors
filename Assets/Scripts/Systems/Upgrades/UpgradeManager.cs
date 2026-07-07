using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [SerializeField] private WeaponSystem weaponSystem;
    [SerializeField] private LevelUpUI levelUpUI;
    [SerializeField] private PlayerEXP expRef;

    private void OnEnable()
    {
        expRef.OnLevelUp += HandleLevelUp;
    }

    private void OnDisable()
    {
        expRef.OnLevelUp -= HandleLevelUp;
    }

    private void HandleLevelUp(int level)
    {
        ShowLevelUp();
    }

    public void ShowLevelUp()
    {
        var choices = GenerateChoices(3);

        if (choices.Count == 0)
        {
            Debug.Log("No upgrades available");
            return;
        }

        levelUpUI.ShowChoices(choices);

        Time.timeScale = 0f;
    }

    public List<WeaponUpgradeOption> GenerateChoices(int count = 3)
    {
        List<WeaponUpgradeOption> candidates = new();

        // Existing weapons
        foreach (var weapon in weaponSystem.Weapons)
        {
            if (weapon.level >= weapon.definition.levels.Length)
                continue;

            candidates.Add(new WeaponUpgradeOption(weapon));
        }

        // Unowned weapons
        foreach (var entry in weaponSystem.AvailableWeapons)
        {
            if (entry.definition == null)
                continue;

            if (entry.level != 0)
                continue;

            candidates.Add(new WeaponUpgradeOption(entry.definition));
        }

        Shuffle(candidates);

        if (candidates.Count > count)
            candidates.RemoveRange(count, candidates.Count - count);

        //List<WeaponUpgradeOption> result = new();

        //int amount = Mathf.Min(count, candidates.Count);

        //for (int i = 0; i < amount; i++)
        //{
        //    result.Add(new WeaponUpgradeOption(candidates[i]));
        //}

        return candidates;
    }

    private void Shuffle(List<WeaponUpgradeOption> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int random = Random.Range(0, i + 1);

            (list[i], list[random]) = (list[random], list[i]);
        }
    }
}
