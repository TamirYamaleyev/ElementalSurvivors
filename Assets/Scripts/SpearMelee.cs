using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpearMelee : MonoBehaviour
{
    [SerializeField] private float damage = 10f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<EnemyAI>(out var enemy))
        {
            enemy.TakeDamage(damage);
        }
    }
}
