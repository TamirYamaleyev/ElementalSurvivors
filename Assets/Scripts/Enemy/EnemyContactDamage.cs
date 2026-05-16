using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
public class EnemyContactDamage : MonoBehaviour
{
    private EnemyStats stats;
    private PlayerHealth playerHealth;
    private bool gameplayEnabled;

    private void Awake()
    {
        stats = GetComponent<EnemyStats>();
    }

    public void EnableGameplay()
    {
        gameplayEnabled = true;

        if (playerHealth == null)
        {
            Transform player = PlayerController.Instance;
            if (player != null)
                playerHealth = player.GetComponent<PlayerHealth>();
        }
    }

    public void DisableGameplay()
    {
        gameplayEnabled = false;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!gameplayEnabled || playerHealth == null)
            return;

        if (other.CompareTag("Player"))
            playerHealth.TakeDamage(stats.RuntimeContactDamage);
    }
}
