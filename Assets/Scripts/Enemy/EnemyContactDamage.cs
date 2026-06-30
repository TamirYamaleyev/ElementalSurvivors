using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public sealed class EnemyContactDamage : MonoBehaviour
{
    private EnemyHealth health;
    private PlayerHealth playerRef;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
    }

    public void SetPlayerTarget(Transform playerTransform)
    {
        playerRef = playerTransform != null
            ? playerTransform.GetComponent<PlayerHealth>()
            : null;
    }

    public void EnsureInitialized(Transform playerTransform)
    {
        if (playerRef != null)
            return;

        SetPlayerTarget(playerTransform ?? PlayerController.Instance);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        GetComponent<EnemyCharacterAnimation>()?.NotifyAttack();

        if (playerRef != null && health != null)
            playerRef.TakeDamage(health.ContactDamage);
    }
}
