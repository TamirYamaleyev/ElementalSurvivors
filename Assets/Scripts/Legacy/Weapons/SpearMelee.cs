using UnityEngine;

public class SpearMelee : MonoBehaviour
{
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float slowDuration = 5f;
    [SerializeField] private float slowMultiplier = 0.5f;
    [SerializeField] private MonoBehaviour statsProviderBehaviour;

    private IPlayerStatsProvider _statsProvider;

    void Awake()
    {
        if (statsProviderBehaviour is IPlayerStatsProvider provider)
            _statsProvider = provider;
        else
            _statsProvider = GetComponentInParent<IPlayerStatsProvider>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!CombatHitUtility.TryResolveEnemyHealth(other, out EnemyHealth health))
            return;

        var ai = other.GetComponentInParent<EnemyAI>();
        if (ai != null)
            ai.ApplySlow(slowDuration, slowMultiplier);

        health.TakeDamage(ResolveDamage());
    }

    private float ResolveDamage()
    {
        if (_statsProvider == null)
            return baseDamage;

        return CombatStatResolver.ScaleDamage(baseDamage, _statsProvider.Current);
    }
}
