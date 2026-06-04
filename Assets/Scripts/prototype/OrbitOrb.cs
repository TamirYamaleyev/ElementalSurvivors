using UnityEngine;

public class OrbitOrb : MonoBehaviour
{
    [SerializeField] private float damage = 1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<EnemyAI>(out var ai))
            ai.ApplyDoT(dotDuration, dotDamage);

        if (other.TryGetComponent<Enemy>(out var enemy))
            enemy.TakeDamage(damage);
    }
}
