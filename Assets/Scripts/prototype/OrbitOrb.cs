using UnityEngine;

public class OrbitOrb : MonoBehaviour
{
    [SerializeField] private float damage = 1f;
    [SerializeField] private float dotDuration = 3f;
    [SerializeField] private float dotDamage = 3f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var ai = other.GetComponent<EnemyAI>() ?? other.GetComponentInParent<EnemyAI>();
        if (ai != null)
            ai.ApplyDoT(dotDuration, dotDamage);

        var enemy = other.GetComponent<Enemy>() ?? other.GetComponentInParent<Enemy>();
        if (enemy != null)
            enemy.TakeDamage(damage);
    }
}
