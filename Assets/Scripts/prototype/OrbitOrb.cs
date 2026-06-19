using UnityEngine;

public class OrbitOrb : MonoBehaviour
{
    [SerializeField] private float damage = 1f;
    [SerializeField] private float dotDuration = 3f;
    [SerializeField] private float dotDamage = 3f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!CombatHitUtility.TryResolveEnemyHealth(other, out EnemyHealth health))
            return;

        var ai = other.GetComponentInParent<EnemyAI>();
        if (ai != null)
            ai.ApplyDoT(dotDuration, dotDamage);

        health.TakeDamage(damage);
    }
}
