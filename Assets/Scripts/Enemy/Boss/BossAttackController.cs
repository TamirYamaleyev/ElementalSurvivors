using System.Collections;
using UnityEngine;

public sealed class BossAttackController : MonoBehaviour, IEnemyPoolReset, IEnemyPoolReset
{
    [Header("Timing")]
    [SerializeField] private float windUpDuration = 1f;
    [SerializeField] private float delayBetweenVolleys = 2f;
    [SerializeField] private float initialDelay = 1.5f;

    [Header("Projectile")]
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private float projectileSpeed = 7f;
    [SerializeField] private float projectileDamage = 12f;
    [SerializeField] private float projectileLifetime = 5f;
    [SerializeField] private Transform firePoint;

    [Header("Patterns")]
    [SerializeField] private BossAttackPatternKind[] patternCycle =
    {
        BossAttackPatternKind.TriangleCone,
        BossAttackPatternKind.SingleLine,
        BossAttackPatternKind.RotatingArc
    };

    [SerializeField] private BossTriangleConeConfig triangleCone = new()
    {
        rows = 7,
        coneHalfAngle = 35f,
        rowSpacing = 0.55f,
        delayBetweenRows = 0.08f
    };

    [SerializeField] private BossSingleLineConfig singleLine = new()
    {
        count = 10,
        spacing = 0.45f,
        delayBetweenShots = 0.06f
    };

    [SerializeField] private BossRotatingArcConfig rotatingArc = new()
    {
        arcAngle = 70f,
        rows = 4,
        projectilesPerRow = 5,
        rowSpacing = 0.5f,
        rotationStepDegrees = 45f
    };

    [Header("References")]
    [SerializeField] private BossAttackTelegraphVfx telegraphVfx;
    [SerializeField] private EnemyCharacterAnimation characterAnimation;

    private int patternIndex;
    private float arcRotation;
    private Coroutine loop;
    private bool attacking;

    public bool IsAttacking => attacking;

    public event System.Action AttackStarted;
    public event System.Action AttackFinished;

    private void Awake()
    {
        if (characterAnimation == null)
            characterAnimation = GetComponent<EnemyCharacterAnimation>();
    }

    private void OnEnable()
    {
        loop = StartCoroutine(VolleyLoop());
    }

    private void OnDisable()
    {
        if (loop != null)
            StopCoroutine(loop);
        loop = null;
        attacking = false;
    }

    public void ResetForPool()
    {
        if (loop != null)
            StopCoroutine(loop);
        loop = null;
        attacking = false;
        patternIndex = 0;
        arcRotation = 0f;
        telegraphVfx?.StopImmediate();
    }

    private IEnumerator VolleyLoop()
    {
        if (initialDelay > 0f)
            yield return new WaitForSeconds(initialDelay);

        while (enabled && gameObject.activeInHierarchy)
        {
            yield return RunVolley();
            if (delayBetweenVolleys > 0f)
                yield return new WaitForSeconds(delayBetweenVolleys);
        }
    }

    private IEnumerator RunVolley()
    {
        if (projectilePrefab == null || patternCycle == null || patternCycle.Length == 0)
            yield break;

        var kind = patternCycle[patternIndex % patternCycle.Length];
        patternIndex++;

        var aim = ResolveAimDirection();
        attacking = true;
        AttackStarted?.Invoke();

        characterAnimation?.NotifyAttack();
        telegraphVfx?.Play(aim);

        if (windUpDuration > 0f)
            yield return new WaitForSeconds(windUpDuration);

        telegraphVfx?.Stop();

        switch (kind)
        {
            case BossAttackPatternKind.TriangleCone:
                yield return FireTriangleCone(aim);
                break;
            case BossAttackPatternKind.SingleLine:
                yield return FireSingleLine(aim);
                break;
            case BossAttackPatternKind.RotatingArc:
                yield return FireRotatingArc();
                arcRotation += rotatingArc.rotationStepDegrees;
                break;
        }

        attacking = false;
        AttackFinished?.Invoke();
    }

    private Vector2 ResolveAimDirection()
    {
        if (PlayerController.Instance == null)
            return Vector2.down;

        var origin = GetFireOrigin();
        var delta = (Vector2)PlayerController.Instance.transform.position - origin;
        return delta.sqrMagnitude > 1e-6f ? delta.normalized : Vector2.down;
    }

    private Vector2 GetFireOrigin()
    {
        return firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
    }

    private void SpawnProjectile(Vector2 origin, Vector2 direction)
    {
        if (direction.sqrMagnitude < 1e-6f)
            direction = Vector2.right;

        EnemyProjectileUtility.Spawn(
            projectilePrefab,
            origin,
            direction,
            projectileDamage,
            projectileSpeed,
            projectileLifetime);
    }

    private IEnumerator FireTriangleCone(Vector2 aim)
    {
        var rows = Mathf.Max(1, triangleCone.rows);
        var baseAngle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;

        for (var row = 0; row < rows; row++)
        {
            var count = row + 1;
            var dist = (row + 1) * triangleCone.rowSpacing;
            var halfAngle = triangleCone.coneHalfAngle * (row + 1) / rows;

            for (var i = 0; i < count; i++)
            {
                var t = count == 1 ? 0.5f : i / (float)(count - 1);
                var angle = baseAngle + Mathf.Lerp(-halfAngle, halfAngle, t);
                var dir = AngleToDirection(angle);
                var origin = GetFireOrigin() + dir * dist * 0.15f;
                SpawnProjectile(origin, dir);
            }

            if (triangleCone.delayBetweenRows > 0f && row < rows - 1)
                yield return new WaitForSeconds(triangleCone.delayBetweenRows);
        }
    }

    private IEnumerator FireSingleLine(Vector2 aim)
    {
        var count = Mathf.Max(1, singleLine.count);
        var origin = GetFireOrigin();

        for (var i = 0; i < count; i++)
        {
            var offset = aim * (i * singleLine.spacing);
            SpawnProjectile(origin + offset, aim);

            if (singleLine.delayBetweenShots > 0f && i < count - 1)
                yield return new WaitForSeconds(singleLine.delayBetweenShots);
        }
    }

    private IEnumerator FireRotatingArc()
    {
        var rows = Mathf.Max(1, rotatingArc.rows);
        var perRow = Mathf.Max(1, rotatingArc.projectilesPerRow);
        var origin = GetFireOrigin();
        var baseAngle = arcRotation;

        for (var row = 0; row < rows; row++)
        {
            var dist = (row + 1) * rotatingArc.rowSpacing;
            var halfArc = rotatingArc.arcAngle * 0.5f;

            for (var i = 0; i < perRow; i++)
            {
                var t = perRow == 1 ? 0.5f : i / (float)(perRow - 1);
                var angle = baseAngle + Mathf.Lerp(-halfArc, halfArc, t);
                var dir = AngleToDirection(angle);
                SpawnProjectile(origin + dir * dist * 0.12f, dir);
            }

            if (triangleCone.delayBetweenRows > 0f && row < rows - 1)
                yield return new WaitForSeconds(0.06f);
        }

        yield break;
    }

    private static Vector2 AngleToDirection(float angleDeg)
    {
        var rad = angleDeg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }
}
