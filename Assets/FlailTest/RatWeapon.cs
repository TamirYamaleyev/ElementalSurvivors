using UnityEngine;

public class RatWeapon : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;

    [SerializeField] private float damageMultiplier = 5f;

    private void OnCollisionEnter2D(Collision2D other)
    {
        float impactSpeed = other.relativeVelocity.magnitude;
        float damage = impactSpeed * damageMultiplier;

        Health target = other.collider.GetComponent<Health>();

        if(target != null)
        {
            target.TakeDamage(damage);
        }
    }
}
