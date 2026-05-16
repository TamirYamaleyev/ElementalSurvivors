using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public event Action<EnemyHealth> Died;

    public float MaxHealth { get; private set; }
    public float CurrentHealth { get; private set; }

    public void Initialize(float maxHp)
    {
        MaxHealth = Mathf.Max(1f, maxHp);
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (CurrentHealth <= 0f)
            return;

        CurrentHealth -= amount;

        if (CurrentHealth <= 0f)
            Died?.Invoke(this);
    }

    public void Clear()
    {
        MaxHealth = 0f;
        CurrentHealth = 0f;
    }
}
