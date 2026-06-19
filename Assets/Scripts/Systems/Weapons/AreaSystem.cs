using UnityEngine;

public class AreaSystem : MonoBehaviour
{
    [SerializeField] private LayerMask enemyLayer;

    public void Cast(
        Vector2 position,
        float radius,
        float damage,
        StatusType status,
        float duration,
        StatusSystem statusSystem,
        Sprite sprite = null)
    {
        CombatHitUtility.ForEachEnemyInArea(position, radius, enemyLayer, enemy =>
        {
            enemy.TakeDamage(damage);
            statusSystem.Apply(enemy, status, duration);
        });
    }
}
