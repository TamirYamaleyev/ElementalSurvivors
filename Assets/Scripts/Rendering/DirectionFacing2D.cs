using UnityEngine;

/// <summary>
/// Aligns a transform's Z rotation so sprite art faces the given world direction.
/// </summary>
public static class DirectionFacing2D
{
    public enum ArtForward
    {
        Right = 0,
        Up = 90,
        Left = 180,
        Down = 270
    }

    public static float GetZRotation(
        Vector2 direction,
        ArtForward artForward = ArtForward.Right,
        float extraOffsetDegrees = 0f)
    {
        float forwardAngle = (float)artForward;

        if (direction.sqrMagnitude < 1e-6f)
            return extraOffsetDegrees - forwardAngle;

        float flightAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return flightAngle - forwardAngle + extraOffsetDegrees;
    }

    public static void Apply(
        Transform target,
        Vector2 direction,
        ArtForward artForward = ArtForward.Right,
        float extraOffsetDegrees = 0f)
    {
        if (target == null)
            return;

        float z = GetZRotation(direction, artForward, extraOffsetDegrees);
        target.rotation = Quaternion.Euler(0f, 0f, z);
    }
}
