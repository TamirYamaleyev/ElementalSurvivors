using System;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public sealed class EnemyDamageNumberPresenter : MonoBehaviour
{
    private EnemyHealth health;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        if (health != null)
            health.OnDamageTaken += HandleDamageTaken;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDamageTaken -= HandleDamageTaken;
    }

    private void HandleDamageTaken(float amount, Vector3 worldPosition, Color? damageColor)
    {
        DamageNumberDisplay.Instance?.DisplayDamageNumber(amount, worldPosition, damageColor);
    }
}
