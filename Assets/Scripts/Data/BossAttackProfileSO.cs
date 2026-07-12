using UnityEngine;

[CreateAssetMenu(
    fileName = "BossAttackProfile",
    menuName = "Elemental Survivors/Boss Attack Profile")]
public sealed class BossAttackProfileSO : ScriptableObject
{
    [Header("Timing")]
    public float windUpDuration = 1f;
    public float delayBetweenVolleys = 2f;
    public float initialDelay = 1.5f;

    [Header("Projectile")]
    public float projectileSpeed = 5f;
    public float projectileDamage = 12f;
    public float projectileLifetime = 5f;

    [Header("Pattern Cycle")]
    public BossAttackPatternKind[] patternCycle =
    {
        BossAttackPatternKind.TriangleCone,
        BossAttackPatternKind.SingleLine,
        BossAttackPatternKind.RotatingArc
    };

    [Header("Triangle Cone")]
    public BossTriangleConeConfig triangleCone = new()
    {
        rows = 7,
        coneHalfAngle = 35f,
        rowSpacing = 0.55f,
        delayBetweenRows = 0.2f
    };

    [Header("Single Line")]
    public BossSingleLineConfig singleLine = new()
    {
        count = 11,
        delayBetweenShots = 0.05f
    };

    [Header("Rotating Arc")]
    public BossRotatingArcConfig rotatingArc = new()
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
}
