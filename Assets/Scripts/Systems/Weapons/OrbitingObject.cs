using UnityEngine;

public class OrbitingObject : MonoBehaviour
{
    [SerializeField] private float spinSpeed = 50f;

    private float angleOffset;
    private float radius;
    private float speed;

    private float damage;
    private StatusType status;
    private float statusDuration;
    private StatusSystem statusSystem;

    private Transform center;

    public void Init(
        int index,
        int total,
        float radius,
        float speed,
        float damage,
        StatusType status,
        float statusDuration,
        StatusSystem statusSystem,
        Transform center
        )

    {
        angleOffset = (Mathf.PI * 2f / total) * index;

        this.radius = radius;
        this.speed = speed;

        this.damage = damage;
        this.status = status;
        this.statusDuration = statusDuration;
        this.statusSystem = statusSystem;

        this.center = center;
    }

    void Update()
    {
        UpdatePosition();
        Spin();
    }

    private void Spin()
    {
        transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
    }

    private void UpdatePosition()
    {
        if (center == null)
            return;

        float angle = Time.time * -speed + angleOffset;

        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        transform.position = (Vector2)center.position + offset;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
      if (other.TryGetComponent(out Enemy enemy))
        {
            enemy.TakeDamage(damage);
            statusSystem.Apply(enemy, status, statusDuration);
        }  
    }
}
