public enum BossAttackPatternKind
{
    TriangleCone,
    SingleLine,
    RotatingArc
}

[System.Serializable]
public struct BossTriangleConeConfig
{
    public int rows;
    public float coneHalfAngle;
    public float rowSpacing;
    public float delayBetweenRows;
}

[System.Serializable]
public struct BossSingleLineConfig
{
    public int count;
    public float delayBetweenShots;
}

[System.Serializable]
public struct BossRotatingArcConfig
{
    public int segmentCount;
    public float segmentArcDegrees;
    public int projectilesPerRow;
    public int radialRows;
    public float rowSpacing;
    public float delayBetweenSegments;
    public float rotationStepDegrees;
    public bool startFromAim;
}
