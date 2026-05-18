using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float iFrameDuration = .5f;

    public event Action<float, float> OnHealthChanged;

    private float currentHealth;
    private float iFrameTimer = 0f;

    void Start()
    {
        currentHealth = maxHealth;    
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Update()
    {
        if (iFrameTimer > 0f)
            iFrameTimer -= Time.deltaTime;
    }

    public void TakeDamage(float amount)
    {
        if (iFrameTimer > 0f)
            return;

        currentHealth -= amount;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        iFrameTimer = iFrameDuration;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("m'dead");
        gameObject.SetActive(false);
    }
}
