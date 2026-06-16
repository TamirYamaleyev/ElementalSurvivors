using UnityEngine;

public class OrbitOrb : MonoBehaviour
{
    [SerializeField] private float damage = 1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var health = other.GetComponent<EnemyHealth>() ?? other.GetComponentInParent<EnemyHealth>();
        if (health != null)
            health.TakeDamage(damage);
    }
}
