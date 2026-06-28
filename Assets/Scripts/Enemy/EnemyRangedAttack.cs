using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyAI))]
public sealed class EnemyRangedAttack : MonoBehaviour, IEnemyPoolReset
{
    [Header("Range")]
    [SerializeField] private float preferredDistance = 5f;
    [SerializeField] private float distanceTolerance = 0.75f;

    [Header("Attack")]
    [SerializeField] private float windUpDuration = 1.2f;
    [SerializeField] private float postFireCooldown = 0.8f;
    [SerializeField] private Transform firePoint;

    [Header("Projectile")]
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private float projectileSpeed = 9f;
    [SerializeField] private float projectileDamage = 8f;
    [SerializeField] private float projectileLifetime = 4f;

    private EnemyAI ai;
    private EnemyCharacterAnimation animation;
    private Coroutine loop;

    private void Awake()
    {
        ai = GetComponent<EnemyAI>();
        animation = GetComponent<EnemyCharacterAnimation>();
    }

    private void OnEnable()
    {
        loop = StartCoroutine(AttackLoop());
    }

    private void OnDisable()
    {
        if (loop != null)
            StopCoroutine(loop);
        loop = null;
    }

    public void ResetForPool()
    {
        if (loop != null)
            StopCoroutine(loop);
        loop = null;
        ai?.SetMovementOverride(true);
        ai?.SetDistanceMaintenance(preferredDistance, distanceTolerance, true);
    }

    private IEnumerator AttackLoop()
    {
        ai.EnsureInitialized();
        ai.SetDistanceMaintenance(preferredDistance, distanceTolerance, true);

        while (enabled && gameObject.activeInHierarchy)
        {
            if (!CanAttack())
            {
                yield return null;
                continue;
            }

            while (CanAttack() && ai.EvaluateDistanceBand() != DistanceBand.InRange)
                yield return null;

            if (!CanAttack())
                continue;

            ai.SetMovementOverride(false);
            animation?.NotifyAttack();

            var elapsed = 0f;
            while (elapsed < windUpDuration)
            {
                if (!CanAttack())
                    break;

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (CanAttack())
                FireAtPlayer();

            ai.SetMovementOverride(true);

            if (postFireCooldown > 0f)
                yield return new WaitForSeconds(postFireCooldown);
        }
    }

    private bool CanAttack()
    {
        return ai != null && ai.IsGameplayEnabled && PlayerController.Instance != null;
    }

    private void FireAtPlayer()
    {
        if (projectilePrefab == null || PlayerController.Instance == null)
            return;

        var origin = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
        var target = (Vector2)PlayerController.Instance.transform.position;
        var dir = target - origin;
        if (dir.sqrMagnitude < 1e-6f)
            dir = Vector2.right;

        EnemyProjectileUtility.Spawn(
            projectilePrefab,
            origin,
            dir,
            projectileDamage,
            projectileSpeed,
            projectileLifetime);
    }
}
