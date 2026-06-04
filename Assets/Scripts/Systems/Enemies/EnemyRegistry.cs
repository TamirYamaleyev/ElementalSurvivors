using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyRegistry : MonoBehaviour
{
    public readonly List<Enemy> ActiveEnemies = new();

    public void Register(Enemy e) /*=> ActiveEnemies.Add(e);*/
    {
        ActiveEnemies.Add(e);
        Debug.Log($"Registered {e.name}");

        foreach (Enemy enemy in ActiveEnemies)
            Debug.Log($"{enemy.name}");
    }
    public void Unregister(Enemy e) => ActiveEnemies.Remove(e);
}
