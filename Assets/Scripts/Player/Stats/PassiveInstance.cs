using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PassiveInstance
{
    public PassiveDefinition definition;

    public int level;

    private readonly List<PlayerStatModifier> modifiers = new();

    public IReadOnlyList<PlayerStatModifier> Modifiers => modifiers;

    public PassiveInstance(PassiveDefinition definition)
    {
        this.definition = definition;
        level = 0;
    }

    public bool IsMaxed =>
        level >= definition.maxLevel;

    public PlayerStatModifier LevelUp()
    {
        if (IsMaxed)
            return default;

        level++;

        float multiplier = Random.Range(definition.minMultiplier, definition.maxMultiplier);

        PlayerStatModifier modifier = new(definition.type, true, multiplier);

        modifiers.Add(modifier);

        return modifier;
    }

}
