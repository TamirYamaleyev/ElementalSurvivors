using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject expOrb;
    [SerializeField] private float maxHealth = 10f;
    [SerializeField] private float contactDamage = 10f;

    private float currentHealth;
    private PlayerHealth playerRef;

    public event Action OnDied;

    public float BaselineMaxHealth => maxHealth;
    public float BaselineContactDamage => contactDamage;

    public void Initialize(Enemy enemy)
    {
        currentHealth = maxHealth;
    }

    public void ApplyScaledStats(float maxHp, float contactDmg)
    {
        maxHealth = maxHp;
        currentHealth = maxHp;
        contactDamage = contactDmg;
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0f)
            return;

        currentHealth -= amount;

        if (currentHealth <= 0f)
            OnDied?.Invoke();
    }

    public void SpawnDeathLoot()
    {
        if (expOrb != null)
            Instantiate(expOrb, transform.position, Quaternion.identity);
    }

    public void EnsureInitialized()
    {
        if (playerRef != null)
            return;

        Transform player = PlayerController.Instance;
        if (player != null)
            playerRef = player.GetComponent<PlayerHealth>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (playerRef != null)
            playerRef.TakeDamage(contactDamage);
    }

    public void ResetState()
    {
        currentHealth = maxHealth;
    }
}
