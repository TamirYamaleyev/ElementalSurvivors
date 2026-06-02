using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private LayerMask enemyLayer;

    private float damage;
    private float speed;
    private Vector2 targetPos;
    private StatusType status;
    private float statusDuration;
    private StatusSystem statusSystem;
    private Transform projectileSpawnPoint;

    public void Init(float damage, float speed, Vector2 targetPos, Transform projectileSpawnPoint, StatusType status, float statusDuration, StatusSystem statusSystem)
    {
        this.damage = damage;
        this.speed = speed;
        this.targetPos = targetPos;
        this.projectileSpawnPoint = projectileSpawnPoint;
        this.status = status;
        this.statusDuration = statusDuration;
        this.statusSystem = statusSystem;
    }

    void Update()
    {
        targetPos != Vector2.zero ? Chase() : FlyDefault();
        
    }

    private void Chase()
    {
        transform.Translate(targetPos * speed * Time.deltaTime);
    }

    private void FlyDefault()
    {
        transform.Translate(projectileSpawnPoint.right * speed * Time.deltaTime);
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
