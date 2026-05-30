using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private LayerMask enemyLayer;

    private float damage;
    private float speed;
    private StatusType status;
    private float statusDuration;
    private StatusSystem statusSystem;

    public void Init(float damage, float speed, StatusType status, float statusDuration, StatusSystem statusSystem)
    {
        this.damage = damage;
        this.speed = speed;
        this.status = status;
        this.statusDuration = statusDuration;
        this.statusSystem = statusSystem;
    }

    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Enemy enemy))
        {
            enemy.TakeDamage(damage);
            statusSystem.Apply(enemy, status, statusDuration);

            // replace with pooling(?)
            Destroy(gameObject);
        }
    }
}
