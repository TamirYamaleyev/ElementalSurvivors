using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpearMelee : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private float slowDuration = 5f;
    [SerializeField] private float slowMultiplier = 0.5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<EnemyAI>(out var enemy))
        {
            enemy.ApplySlow(slowDuration, slowMultiplier);
            enemy.TakeDamage(damage);
        }
    }
}
