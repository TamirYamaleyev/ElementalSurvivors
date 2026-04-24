using System.Collections.Generic;
using UnityEngine;

public class ActionValueQueue
{
    private readonly List<CombatEntity> entities = new List<CombatEntity>();

    public void Initialize(IEnumerable<CombatEntity> combatants)
    {
        entities.Clear();
        entities.AddRange(combatants);

        for (int i = 0; i < entities.Count; i++)
            entities[i].ResetActionValue();
    }

    public CombatEntity AdvanceToNextActor()
    {
        CleanupDead();
        if (entities.Count == 0)
            return null;

        float minActionValue = float.MaxValue;
        for (int i = 0; i < entities.Count; i++)
            minActionValue = Mathf.Min(minActionValue, entities[i].CurrentActionValue);

        for (int i = 0; i < entities.Count; i++)
            entities[i].ReduceActionValue(minActionValue);

        CombatEntity actor = entities[0];
        for (int i = 1; i < entities.Count; i++)
        {
            if (entities[i].CurrentActionValue < actor.CurrentActionValue)
                actor = entities[i];
        }

        return actor;
    }

    public void CompleteTurn(CombatEntity actor)
    {
        if (actor == null)
            return;

        actor.ResetActionValue();
        CleanupDead();
    }

    public IReadOnlyList<CombatEntity> Snapshot()
    {
        return entities;
    }

    private void CleanupDead()
    {
        entities.RemoveAll(e => e == null || e.IsDead);
    }
}
