using System.Collections.Generic;
using UnityEngine;

public class EnemyRegistry : MonoBehaviour
{
    public readonly List<Enemy> ActiveEnemies = new();

    public void Register(Enemy e) => ActiveEnemies.Add(e);
    public void Unregister(Enemy e) => ActiveEnemies.Remove(e);
}
