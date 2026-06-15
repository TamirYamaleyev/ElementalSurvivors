using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("TEMPORARY SHIT TO REMOVE")]
    [SerializeField] private GameObject TEMPORARYEXPORB;

    [SerializeField] private float maxHealth = 10f;
    private float currentHealth;

    private Enemy owner;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    /// <summary>Called after spawn when <see cref="EnemyAI.ApplyScaledStats"/> scales difficulty.</summary>
    public void ApplySpawnScaling(float maxHp)
    {
        maxHealth = maxHp;
        currentHealth = maxHp;
    }

    public void Initialize(Enemy enemy)
    {
        owner = enemy;
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Instantiate(TEMPORARYEXPORB, transform.position, Quaternion.identity);

        var driver = GetComponentInChildren<EnemyCharacterAnimation>();
        if (driver != null)
        {
            driver.BeginDeathSequence();
            return;
        }

        Destroy(gameObject);
    }
}
