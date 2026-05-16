using UnityEngine;

public class OrbitOrb : MonoBehaviour
{
    [SerializeField] private float damage = 1f;
    [SerializeField] private float dotDuration = 3f;
    [SerializeField] private float dotDamage = 3f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<IDamageable>(out var damageable))
            return;

        damageable.TakeDamage(damage);

        if (other.TryGetComponent<IStatusEffectTarget>(out var effects))
            effects.ApplyDoT(dotDuration, dotDamage);
    }
}
