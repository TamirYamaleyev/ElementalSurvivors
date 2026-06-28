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
    public float spacing;
    public float delayBetweenShots;
}

[System.Serializable]
public struct BossRotatingArcConfig
{
    public float arcAngle;
    public int rows;
    public int projectilesPerRow;
    public float rowSpacing;
    public float rotationStepDegrees;
}
