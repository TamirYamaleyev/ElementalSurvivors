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
        Debug.Log("Registered enemy");
    }
    public void Unregister(Enemy e) => ActiveEnemies.Remove(e);
}
