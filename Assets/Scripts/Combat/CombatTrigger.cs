using UnityEngine;

public class CombatTrigger : MonoBehaviour
{
    [SerializeField] private EnemyAI enemyAI;

    private void Awake()
    {
        if (enemyAI == null)
            enemyAI = GetComponent<EnemyAI>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (enemyAI == null || !other.CompareTag("Player"))
            return;

        EnemyAI.RequestCombat(enemyAI);
    }
}
