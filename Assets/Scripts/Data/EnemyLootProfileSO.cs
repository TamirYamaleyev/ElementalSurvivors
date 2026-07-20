using UnityEngine;

[CreateAssetMenu(fileName = "EnemyLootProfile", menuName = "Elemental Survivors/Enemy Loot Profile")]
public class EnemyLootProfileSO : ScriptableObject
{
    [Header("Prefabs")]
    public GameObject expOrbPrefab;
    public GameObject healthOrbPrefab;

    [Header("Roll")]
    [Tooltip("EXP orb is always dropped when assigned. This is the chance for an extra health orb on the same kill.")]
    [Range(0f, 1f)]
    public float healthOrbChance = 0.5f;

    [Tooltip("Horizontal offset for the health orb when both EXP and health spawn, so they do not overlap.")]
    [SerializeField] private float bonusHealthOrbOffset = 0.35f;

    public void SpawnLoot(Vector3 position, Enemy enemy)
    {
        bool spawnedExp = expOrbPrefab != null;
        if (spawnedExp)
        {
            EXPOrb orb = Instantiate(expOrbPrefab, position, Quaternion.identity).GetComponent<EXPOrb>();
            orb.expToGive = enemy.ExpReward;
        }

        if (healthOrbPrefab != null && Random.value < healthOrbChance)
        {
            Vector3 healthPos = spawnedExp
                ? position + new Vector3(bonusHealthOrbOffset, 0f, 0f)
                : position;
            Instantiate(healthOrbPrefab, healthPos, Quaternion.identity);
        }
    }
}
