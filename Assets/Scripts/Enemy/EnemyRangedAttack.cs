using System.Collections;
using UnityEngine;

/// <summary>
/// Ranged enemy attack: approach to distance X, channel Y seconds, fire one boss-style projectile at the player.
/// Movement is delegated to <see cref="EnemyAI"/> via distance maintenance and movement override.
/// </summary>
[RequireComponent(typeof(EnemyAI))]
[DefaultExecutionOrder(100)]
public sealed class EnemyRangedAttack : MonoBehaviour
{
    [Header("Range")]
    [Tooltip("Distance X from the player before the enemy stops and channels a shot.")]
    [SerializeField] private float preferredDistance = 5f;
    [SerializeField] private float distanceTolerance = 0.75f;

    [Header("Attack")]
    [Tooltip("Wind-up / channel duration Y before firing.")]
    [SerializeField] private float windUpDuration = 1.2f;
    [SerializeField] private float postFireCooldown = 0.8f;
    [SerializeField] private Transform firePoint;

    [Header("Projectile")]
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private float projectileSpeed = 9f;
    [SerializeField] private float projectileDamage = 8f;
    [SerializeField] private float projectileLifetime = 4f;

    private EnemyAI ai;
    private EnemyCharacterAnimation characterAnimation;
    private SpriteRenderer bodySprite;
    private Transform playerTarget;
    private Coroutine loop;
    private bool pendingFire;
    private float firePointAbsX;

    private void Awake()
    {
        ai = GetComponent<EnemyAI>();
        characterAnimation = GetComponent<EnemyCharacterAnimation>();
        bodySprite = GetComponentInChildren<SpriteRenderer>(true);

        if (firePoint != null)
            firePointAbsX = Mathf.Abs(firePoint.localPosition.x);
    }

    public void SetPlayerTarget(Transform playerTransform)
    {
        playerTarget = playerTransform;
    }

    private Transform ResolvePlayerTarget()
    {
        if (playerTarget != null)
            return playerTarget;

        return PlayerController.Instance;
    }

    private void OnEnable()
    {
        loop = StartCoroutine(AttackLoop());
    }

    private void OnDisable()
    {
        pendingFire = false;
        StopAttackLoop();
    }

    private void LateUpdate()
    {
        SyncFirePointTowardPlayer();
    }

    private void SyncFirePointTowardPlayer()
    {
        if (firePoint == null || firePointAbsX <= 0f)
            return;

        var flip = bodySprite != null && bodySprite.flipX;
        var local = firePoint.localPosition;
        local.x = firePointAbsX * (flip ? -1f : 1f);
        firePoint.localPosition = local;
    }

    private void StopAttackLoop()
    {
        if (loop != null)
            StopCoroutine(loop);
        loop = null;
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

            yield return ChannelAndFire();

            ai.SetMovementOverride(true);

            if (postFireCooldown > 0f)
                yield return new WaitForSeconds(postFireCooldown);
        }
    }

    private IEnumerator ChannelAndFire()
    {
        ai.SetMovementOverride(false);
        pendingFire = true;
        characterAnimation?.NotifyAttack();

        var elapsed = 0f;
        while (pendingFire && elapsed < windUpDuration)
        {
            if (!CanAttack())
            {
                pendingFire = false;
                ai.SetMovementOverride(true);
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (pendingFire)
        {
            pendingFire = false;
            if (CanAttack())
                FireAtPlayer();
        }
    }

    private bool CanAttack()
    {
        return ai != null && ai.IsGameplayEnabled && ResolvePlayerTarget() != null;
    }

    private void FireAtPlayer()
    {
        var player = ResolvePlayerTarget();
        if (projectilePrefab == null || player == null)
            return;

        var origin = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
        var target = (Vector2)player.position;
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
