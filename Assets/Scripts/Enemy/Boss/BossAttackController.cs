using System.Collections;
using UnityEngine;

public sealed class BossAttackController : MonoBehaviour, IEnemyPoolReset
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
        count = 11,
        delayBetweenShots = 0.05f
    };

    [SerializeField] private BossRotatingArcConfig rotatingArc = new()
    {
        segmentCount = 5,
        segmentArcDegrees = 72f,
        projectilesPerRow = 9,
        radialRows = 6,
        rowSpacing = 0.5f,
        delayBetweenSegments = 0.5f,
        rotationStepDegrees = 45f,
        startFromAim = true
    };

    [Header("References")]
    [SerializeField] private BossAttackTelegraphVfx telegraphVfx;
    [SerializeField] private EnemyCharacterAnimation characterAnimation;

    private int patternIndex;
    private float arcRotation;
    private Coroutine loop;
    private bool attacking;
    private EnemyAI ai;

    public bool IsAttacking => attacking;

    public event System.Action AttackStarted;
    public event System.Action AttackFinished;

    private void Awake()
    {
        ai = GetComponent<EnemyAI>();
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
            while (ai != null && !ai.IsGameplayEnabled)
                yield return null;

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

        var telegraphDir = kind == BossAttackPatternKind.RotatingArc
            ? AngleToDirection(ResolveFirstSegmentCenterAngle(aim))
            : aim;
        telegraphVfx?.Play(telegraphDir);

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
                yield return FireRotatingArc(aim);
                arcRotation = Mathf.Repeat(arcRotation + rotatingArc.rotationStepDegrees, 360f);
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

    private float ResolveFirstSegmentCenterAngle(Vector2 aim)
    {
        var aimAngle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
        var baseAngle = rotatingArc.startFromAim ? aimAngle : 0f;
        return baseAngle + arcRotation;
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
        var direction = aim.sqrMagnitude > 1e-6f ? aim.normalized : Vector2.down;

        for (var i = 0; i < count; i++)
        {
            SpawnProjectile(origin, direction);

            if (singleLine.delayBetweenShots > 0f && i < count - 1)
                yield return new WaitForSeconds(singleLine.delayBetweenShots);
        }
    }

    private IEnumerator FireRotatingArc(Vector2 aim)
    {
        var segments = Mathf.Max(1, rotatingArc.segmentCount);
        var segmentArc = rotatingArc.segmentArcDegrees > 0f
            ? rotatingArc.segmentArcDegrees
            : 360f / segments;
        var radialRows = Mathf.Max(1, rotatingArc.radialRows);
        var perRow = Mathf.Max(1, rotatingArc.projectilesPerRow);
        var origin = GetFireOrigin();
        var aimAngle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
        var baseAngle = rotatingArc.startFromAim ? aimAngle : 0f;
        var halfSegment = segmentArc * 0.5f;
        var sweepSign = Random.value < 0.5f ? 1f : -1f;

        for (var seg = 0; seg < segments; seg++)
        {
            var center = baseAngle + arcRotation + seg * segmentArc * sweepSign;
            telegraphVfx?.SetDirection(AngleToDirection(center));

            for (var radial = 0; radial < radialRows; radial++)
            {
                var dist = (radial + 1) * rotatingArc.rowSpacing;

                for (var i = 0; i < perRow; i++)
                {
                    var t = perRow == 1 ? 0.5f : i / (float)(perRow - 1);
                    var angle = center + Mathf.Lerp(-halfSegment, halfSegment, t);
                    var dir = AngleToDirection(angle);
                    SpawnProjectile(origin + dir * dist * 0.12f, dir);
                }
            }

            if (rotatingArc.delayBetweenSegments > 0f && seg < segments - 1)
                yield return new WaitForSeconds(rotatingArc.delayBetweenSegments);
        }
    }

    private static Vector2 AngleToDirection(float angleDeg)
    {
        var rad = angleDeg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }
}
