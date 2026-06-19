using UnityEngine;

public class DefaultWeaponProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private DirectionFacing2D.ArtForward artForward = DirectionFacing2D.ArtForward.Right;
    [SerializeField] private float extraRotationOffset;

    private Vector2 direction;

    public void Init(Vector2 dir)
    {
        direction = dir.sqrMagnitude > 1e-6f ? dir.normalized : Vector2.right;
        DirectionFacing2D.Apply(transform, direction, artForward, extraRotationOffset);
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    void OnEnable()
    {
        Invoke(nameof(Disable), lifetime);
    }

    void OnDisable()
    {
        CancelInvoke();
    }

    private void Disable()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Enemy>(out var enemy))
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
