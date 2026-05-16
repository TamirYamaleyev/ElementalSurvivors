using UnityEngine;

public class SpearMelee : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private float slowDuration = 5f;
    [SerializeField] private float slowMultiplier = 0.5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<IDamageable>(out var damageable))
            return;

        damageable.TakeDamage(damage);

        if (other.TryGetComponent<IStatusEffectTarget>(out var effects))
            effects.ApplySlow(slowDuration, slowMultiplier);
    }
}
