using UnityEngine;

/// <summary>
/// Samples spawn points outside the camera viewport (with margin), independent of pool/difficulty.
/// </summary>
public static class EnemyOffscreenSpawnSampler
{
    const float DefaultFallbackSpawnRadius = 10f;
    const float EdgeEpsilon = 0.05f;

    public static bool TryGetCameraWorldRect(Camera cam, out Rect worldRect)
    {
        worldRect = default;
        if (cam == null)
            return false;

        float z = -cam.transform.position.z;
        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0f, 0f, z));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1f, 1f, z));

        float xMin = Mathf.Min(bottomLeft.x, topRight.x);
        float yMin = Mathf.Min(bottomLeft.y, topRight.y);
        float width = Mathf.Abs(topRight.x - bottomLeft.x);
        float height = Mathf.Abs(topRight.y - bottomLeft.y);

        if (width <= 0.01f || height <= 0.01f)
            return false;

        worldRect = new Rect(xMin, yMin, width, height);
        return true;
    }

    /// <summary>
    /// Point just outside the padded viewport AABB, then pushed outward by [0, bandWidth].
    /// Fallback: random disk around <paramref name="fallbackOrigin"/> if camera is unavailable.
    /// </summary>
    public static Vector3 SampleOutsideViewport(
        Camera cam,
        Vector3 fallbackOrigin,
        float margin,
        float bandWidth,
        float jitter = 0f,
        float fallbackRadius = DefaultFallbackSpawnRadius)
    {
        if (!TryGetCameraWorldRect(cam, out var viewRect))
        {
            Vector2 disk = Random.insideUnitCircle.normalized;
            if (disk.sqrMagnitude < 1e-6f)
                disk = Vector2.right;
            float baseR = fallbackRadius > 0f ? fallbackRadius : DefaultFallbackSpawnRadius;
            float r = baseR + Random.Range(0f, Mathf.Max(0f, bandWidth));
            return fallbackOrigin + (Vector3)(disk * r);
        }

        GetExpandedRect(viewRect, margin, out var expanded);
        Vector2 center = expanded.center;
        float angle = Random.Range(0f, Mathf.PI * 2f);
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        if (dir.sqrMagnitude < 1e-6f)
            dir = Vector2.right;
        dir.Normalize();

        Vector2 edge = RayExpandedRectEdge(center, dir, expanded);
        float outward = EdgeEpsilon + Random.Range(0f, Mathf.Max(0f, bandWidth));
        Vector2 point = edge + dir * outward;

        if (jitter > 0f)
        {
            Vector2 tangent = new Vector2(-dir.y, dir.x);
            point += tangent * Random.Range(-jitter, jitter);
            point += dir * Random.Range(-jitter * 0.25f, jitter * 0.25f);
        }

        if (IsInsideInclusive(expanded, point))
            point = edge + dir * Mathf.Max(outward, EdgeEpsilon);

        return new Vector3(point.x, point.y, fallbackOrigin.z);
    }

    /// <summary>
    /// Deterministic ring sample outside the padded viewport (for clearance fallback).
    /// </summary>
    public static Vector3 SampleOutsideViewportRing(
        Camera cam,
        Vector3 fallbackOrigin,
        float margin,
        float bandWidth,
        float angleRadians,
        float bandT,
        float fallbackRadius = DefaultFallbackSpawnRadius)
    {
        bandT = Mathf.Clamp01(bandT);

        if (!TryGetCameraWorldRect(cam, out var viewRect))
        {
            Vector2 dir = new Vector2(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));
            float baseR = fallbackRadius > 0f ? fallbackRadius : DefaultFallbackSpawnRadius;
            float r = baseR + bandT * Mathf.Max(0f, bandWidth);
            return fallbackOrigin + (Vector3)(dir * r);
        }

        GetExpandedRect(viewRect, margin, out var expanded);
        Vector2 center = expanded.center;
        Vector2 dir2 = new Vector2(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));
        if (dir2.sqrMagnitude < 1e-6f)
            dir2 = Vector2.right;
        dir2.Normalize();

        Vector2 edge = RayExpandedRectEdge(center, dir2, expanded);
        float outward = EdgeEpsilon + bandT * Mathf.Max(0f, bandWidth);
        Vector2 point = edge + dir2 * outward;
        return new Vector3(point.x, point.y, fallbackOrigin.z);
    }

    public static bool IsInsidePaddedViewport(Camera cam, Vector3 worldPos, float margin)
    {
        if (!TryGetCameraWorldRect(cam, out var viewRect))
            return false;

        GetExpandedRect(viewRect, margin, out var expanded);
        return IsInsideInclusive(expanded, new Vector2(worldPos.x, worldPos.y));
    }

    static void GetExpandedRect(Rect viewRect, float margin, out Rect expanded)
    {
        float pad = Mathf.Max(0f, margin);
        expanded = new Rect(
            viewRect.xMin - pad,
            viewRect.yMin - pad,
            viewRect.width + pad * 2f,
            viewRect.height + pad * 2f);
    }

    static bool IsInsideInclusive(Rect rect, Vector2 point)
    {
        return point.x >= rect.xMin && point.x <= rect.xMax &&
               point.y >= rect.yMin && point.y <= rect.yMax;
    }

    static Vector2 RayExpandedRectEdge(Vector2 origin, Vector2 dir, Rect rect)
    {
        float tMin = float.PositiveInfinity;

        if (Mathf.Abs(dir.x) > 1e-6f)
        {
            float tx1 = (rect.xMin - origin.x) / dir.x;
            float tx2 = (rect.xMax - origin.x) / dir.x;
            if (tx1 > 0f) tMin = Mathf.Min(tMin, tx1);
            if (tx2 > 0f) tMin = Mathf.Min(tMin, tx2);
        }

        if (Mathf.Abs(dir.y) > 1e-6f)
        {
            float ty1 = (rect.yMin - origin.y) / dir.y;
            float ty2 = (rect.yMax - origin.y) / dir.y;
            if (ty1 > 0f) tMin = Mathf.Min(tMin, ty1);
            if (ty2 > 0f) tMin = Mathf.Min(tMin, ty2);
        }

        if (float.IsInfinity(tMin) || tMin <= 0f)
        {
            float halfW = rect.width * 0.5f;
            float halfH = rect.height * 0.5f;
            tMin = Mathf.Sqrt(halfW * halfW + halfH * halfH);
        }

        return origin + dir * tMin;
    }
}
