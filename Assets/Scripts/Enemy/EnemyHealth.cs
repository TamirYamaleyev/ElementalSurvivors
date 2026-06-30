using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyLootProfileSO lootProfile;
    [SerializeField] private float maxHealth = 10f;
    [SerializeField] private float contactDamage = 10f;

    private float currentHealth;

    public event Action OnDied;
    public event Action<float, float> OnHealthChanged;
    public event Action<float, Vector3, Color?> OnDamageTaken;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float BaselineMaxHealth => maxHealth;
    public float BaselineContactDamage => contactDamage;
    public float ContactDamage => contactDamage;
    public float LastDamageReceived { get; private set; }

    public void Initialize(Enemy enemy)
    {
        currentHealth = maxHealth;
        NotifyHealthChanged();
    }

    /// <summary>Called after spawn when difficulty scaling sets max HP before full stats are applied.</summary>
    public void ApplySpawnScaling(float maxHp)
    {
        maxHealth = maxHp;
        currentHealth = maxHp;
        NotifyHealthChanged();
    }

    public void ApplyScaledStats(float maxHp, float contactDmg)
    {
        maxHealth = maxHp;
        currentHealth = maxHp;
        contactDamage = contactDmg;
        NotifyHealthChanged();
    }

    public void TakeDamage(float amount)
    {
        TakeDamage(amount, null);
    }

    public void TakeDamage(float amount, Color? damageColor)
    {
        if (currentHealth <= 0f || amount <= 0f)
            return;

        LastDamageReceived = amount;
        currentHealth -= amount;

        OnDamageTaken?.Invoke(amount, transform.position, damageColor);
        NotifyHealthChanged();

        if (currentHealth <= 0f)
            OnDied?.Invoke();
    }

    public void SpawnDeathLoot()
    {
        if (lootProfile != null)
            lootProfile.SpawnLoot(transform.position);
    }

    public void ResetState()
    {
        currentHealth = maxHealth;
        LastDamageReceived = 0f;
        NotifyHealthChanged();
    }

    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
