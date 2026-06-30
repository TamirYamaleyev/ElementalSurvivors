using UnityEngine;

public interface IWeaponExecutionStrategy
{
    bool Execute(
        Enemy target,
        WeaponSystemContext ctx,
        WeaponLevelData data,
        WeaponDefinition definition,
        float damage,
        float speed,
        Vector2 spawnPos);
}
