using UnityEngine;

public class SpearMelee : MonoBehaviour
{
    [SerializeField] private float damage = 10f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var health = other.GetComponent<EnemyHealth>() ?? other.GetComponentInParent<EnemyHealth>();
        if (health != null)
            health.TakeDamage(damage);
    }
}
