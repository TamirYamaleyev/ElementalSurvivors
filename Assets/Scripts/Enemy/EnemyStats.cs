using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [SerializeField] private EnemyStatsSO baseline;

    public float BaselineMaxHealth => baseline != null ? baseline.maxHealth : 1f;
    public float BaselineContactDamage => baseline != null ? baseline.contactDamage : 1f;
    public float MoveSpeed => baseline != null ? baseline.moveSpeed : 3f;

    public float RuntimeMaxHealth { get; private set; }
    public float RuntimeContactDamage { get; private set; }

    public void SetRuntime(float maxHp, float contactDamage)
    {
        RuntimeMaxHealth = maxHp;
        RuntimeContactDamage = contactDamage;
    }

    public void ClearRuntime()
    {
        RuntimeMaxHealth = 0f;
        RuntimeContactDamage = 0f;
    }
}
