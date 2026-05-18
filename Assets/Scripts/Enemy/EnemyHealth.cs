using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("TEMPORARY SHIT TO REMOVE")]
    [SerializeField] private GameObject TEMPORARYEXPORB;

    [SerializeField] private float maxHealth = 10f;
    private float currentHealth;

    private Enemy owner;

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
        Destroy(gameObject);
    }
}
