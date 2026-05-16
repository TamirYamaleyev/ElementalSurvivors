using UnityEngine;

public class BoomerangWeapon : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float maxDistance = 6f;
    [SerializeField] private float returnSpeedMultiplier = 1.4f;

    [Header("Combat")]
    [SerializeField] private float damage = 1f;
    [SerializeField] private float fearDuration = 1.5f;

    private Transform owner;
    private Vector3 startPosition;
    private Vector3 moveDir;
    private bool returning;

    public void Init(Transform player, Vector3 direction)
    {
        owner = player;
        startPosition = transform.position;

        moveDir = direction.normalized;
        returning = false;

        transform.right = moveDir;
    }

    void Update()
    {
        if (owner == null) return;

        if (!returning)
        {
            transform.position += moveDir * speed * Time.deltaTime;

            float traveled = Vector3.Distance(startPosition, transform.position);

            if (traveled >= maxDistance)
                returning = true;
        }
        else
        {
            Vector3 dirToOwner = (owner.position - transform.position).normalized;

            transform.position += dirToOwner * speed * returnSpeedMultiplier * Time.deltaTime;

            transform.right = dirToOwner;

            if (Vector3.Distance(transform.position, owner.position) < 0.5f)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<IDamageable>(out var damageable))
            return;

        damageable.TakeDamage(damage);

        if (other.TryGetComponent<IStatusEffectTarget>(out var effects))
            effects.ApplyFear(fearDuration);
    }
}