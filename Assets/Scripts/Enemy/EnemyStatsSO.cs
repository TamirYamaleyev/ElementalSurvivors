using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Elemental Survivors/Enemy/Enemy Stats")]
public class EnemyStatsSO : ScriptableObject
{
    public float maxHealth = 10f;
    public float contactDamage = 10f;
    public float moveSpeed = 3f;
}
