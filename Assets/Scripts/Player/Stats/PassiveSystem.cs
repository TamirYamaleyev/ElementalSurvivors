using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PassiveSystem : MonoBehaviour
{
    [Header("Available Passives")]
    [SerializeField] private List<PassiveDefinition> availablePassives;

    private readonly List<PassiveInstance> ownedPassives = new();

    private PlayerStats playerStats;

    public IReadOnlyList<PassiveInstance> Passives => ownedPassives;

    public IReadOnlyList<PassiveDefinition> AvailablePassives => availablePassives;

    public void Initialize(PlayerStats stats)
    {
        playerStats = stats;

        ownedPassives.Clear();

        // ??
        //foreach (var definition in availablePassives)
        //{
        //    ownedPassives.Add(new PassiveInstance(definition));
        //}
    }

    public PassiveInstance GetOrCreatePassive(PassiveDefinition definition)
    {
        foreach (var passive in ownedPassives)
        {
            if (passive.definition == definition)
                return passive;
        }

        var instance = new PassiveInstance(definition);

        ownedPassives.Add(instance);

        return instance;
    }

    public void UpgradePassive(PassiveInstance passive)
    {
        PlayerStatModifier modifier = passive.LevelUp();

        playerStats.ApplyModifier(modifier);
    }
}
