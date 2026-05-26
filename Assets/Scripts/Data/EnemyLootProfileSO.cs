using UnityEngine;

[CreateAssetMenu(fileName = "EnemyLootProfile", menuName = "Elemental Survivors/Enemy Loot Profile")]
public class EnemyLootProfileSO : ScriptableObject
{
    [Header("Prefabs")]
    public GameObject expOrbPrefab;
    public GameObject healthOrbPrefab;

    [Header("Roll")]
    [Tooltip("Chance to drop health orb instead of EXP (single drop per death).")]
    [Range(0f, 1f)]
    public float healthOrbChance = 0.5f;

    public void SpawnLoot(Vector3 position)
    {
        bool wantHealth = Random.value < healthOrbChance;

        if (wantHealth && healthOrbPrefab != null)
        {
            Instantiate(healthOrbPrefab, position, Quaternion.identity);
            return;
        }

        if (expOrbPrefab != null)
            Instantiate(expOrbPrefab, position, Quaternion.identity);
        else if (healthOrbPrefab != null)
            Instantiate(healthOrbPrefab, position, Quaternion.identity);
    }
}
