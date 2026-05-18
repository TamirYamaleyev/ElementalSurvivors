using UnityEngine;

public class AreaSystem : MonoBehaviour
{
    [SerializeField] private LayerMask enemyLayer;
    public void Cast(Vector2 position, float radius, float damage, StatusType status, float duration, StatusSystem statusSystem)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, radius, enemyLayer);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out Enemy enemy))
            {
                enemy.TakeDamage(damage);
                statusSystem.Apply(enemy, status, duration);
            }
        }
    }
}
