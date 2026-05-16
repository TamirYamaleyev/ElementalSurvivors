using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyDeathLoot : MonoBehaviour
{
    [SerializeField] private GameObject expOrb;

    private EnemyHealth health;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        if (health != null)
            health.Died += OnDied;
    }

    private void OnDisable()
    {
        if (health != null)
            health.Died -= OnDied;
    }

    private void OnDied(EnemyHealth _)
    {
        if (expOrb != null)
            Instantiate(expOrb, transform.position, Quaternion.identity);
    }
}
