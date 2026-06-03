using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private EnemyAI ai;
    [SerializeField] private EnemyStatusController status;
    [SerializeField] private EnemyHealth health;

    public EnemyStatusController StatusController => status;

    private void Awake()
    {
        if (status == null)
            status = GetComponent<EnemyStatusController>();
        if (health == null)
            health = GetComponent<EnemyHealth>();

        if (status == null || health == null)
            return;

        var sys = FindAnyObjectByType<StatusSystem>();
        if (sys != null)
            status.Initialize(sys, this);

        health.Initialize(this);
    }

    public void Initialize(StatusSystem statusSystem, EnemyRegistry registry)
    {
        if (status == null)
            status = GetComponent<EnemyStatusController>();
        if (health == null)
            health = GetComponent<EnemyHealth>();

        if (status != null)
            status.Initialize(statusSystem, this);
        if (health != null)
            health.Initialize(this);

        registry.Register(this);
    }

    public void TakeDamage(float amount)
    {
        health.TakeDamage(amount);
    }
}
