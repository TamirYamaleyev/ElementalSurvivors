using UnityEngine;

public class OrbitOrb : MonoBehaviour
{
    [SerializeField] private float damage = 1f;
    [SerializeField] private float dotDuration = 3f;
    [SerializeField] private float dotDamage = 3f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var health = other.GetComponent<EnemyHealth>() ?? other.GetComponentInParent<EnemyHealth>();
        if (health == null)
            return;

        var ai = other.GetComponentInParent<EnemyAI>();
        if (ai != null)
            ai.ApplyDoT(dotDuration, dotDamage);

        health.TakeDamage(damage);
    }
}
