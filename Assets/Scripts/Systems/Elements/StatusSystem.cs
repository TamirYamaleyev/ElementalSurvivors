using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class StatusSystem : MonoBehaviour
{
    public void Apply(Enemy enemy, StatusType type, float duration)
    {
        enemy.StatusController.AddStatus(type, duration);
    }

    public void ResolveInteractions(Enemy enemy, List<StatusInstance> existing, StatusInstance incoming)
    {
        foreach (var s in existing)
        {
            if (s.type == incoming.type)
                continue;

            TryTriggerInteraction(enemy, s.type, incoming.type);
        }
    }

    private void TryTriggerInteraction(Enemy enemy, StatusType a, StatusType b)
    {
        //placeholder for interactions
        Debug.Log($"Interaction: {a} + {b}");
    }
}
