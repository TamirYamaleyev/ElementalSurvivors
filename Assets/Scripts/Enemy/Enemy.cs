using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private EnemyAI ai;
    [SerializeField] private EnemyStatusController status;
    [SerializeField] private EnemyHealth health;

    public EnemyStatusController StatusController => status;

    public void Initialize(StatusSystem statusSystem, EnemyRegistry registry)
    {
        status.Initialize(statusSystem, this);
        health.Initialize(this);

        registry.Register(this);
    }

    public void TakeDamage(float amount)
    {
        health.TakeDamage(amount);
    }
}
