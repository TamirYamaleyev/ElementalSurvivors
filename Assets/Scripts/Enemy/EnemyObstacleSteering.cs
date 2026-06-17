using UnityEngine;

public static class EnemyObstacleSteering
{
    const float Skin = 0.01f;

    public static Vector2 ResolveSteerDirection(
        Vector2 origin,
        float castRadius,
        Vector2 desiredDirection,
        float probeDistance,
        LayerMask obstacleMask,
        float[] steerAnglesDeg)
    {
        if (desiredDirection.sqrMagnitude < 1e-6f)
            return Vector2.zero;

        desiredDirection.Normalize();

        if (steerAnglesDeg == null || steerAnglesDeg.Length == 0)
            steerAnglesDeg = new[] { 0f };

        foreach (float angle in steerAnglesDeg)
        {
            Vector2 probeDir = Rotate(desiredDirection, angle);
            if (!IsBlocked(origin, castRadius, probeDir, probeDistance, obstacleMask))
                return probeDir;
        }

        return desiredDirection;
    }

    public static Vector2 MoveWithCollision(
        Rigidbody2D rb,
        float castRadius,
        Vector2 direction,
        float speed,
        LayerMask obstacleMask)
    {
        if (rb == null || direction.sqrMagnitude < 1e-6f || speed <= 0f)
            return rb != null ? rb.position : Vector2.zero;

        direction.Normalize();
        Vector2 delta = direction * speed * Time.fixedDeltaTime;
        Vector2 origin = rb.position;

        RaycastHit2D hit = Physics2D.CircleCast(
            origin,
            castRadius,
            delta.normalized,
            delta.magnitude + Skin,
            obstacleMask);

        if (hit.collider != null)
        {
            float travel = Mathf.Max(0f, hit.distance - Skin);
            delta = delta.normalized * travel;

            Vector2 remainder = direction * speed * Time.fixedDeltaTime - delta;
            if (remainder.sqrMagnitude > 1e-6f)
            {
                Vector2 slide = Vector2.Perpendicular(hit.normal);
                if (Vector2.Dot(slide, direction) < 0f)
                    slide = -slide;

                RaycastHit2D slideHit = Physics2D.CircleCast(
                    origin + delta,
                    castRadius,
                    slide.normalized,
                    remainder.magnitude + Skin,
                    obstacleMask);

                if (slideHit.collider == null)
                    delta += slide.normalized * remainder.magnitude;
                else
                    delta += slide.normalized * Mathf.Max(0f, slideHit.distance - Skin);
            }
        }

        Vector2 next = origin + delta;
        rb.MovePosition(next);
        rb.linearVelocity = Vector2.zero;
        return next;
    }

    public static void SeparateFromObstacles(Rigidbody2D rb, float castRadius, LayerMask obstacleMask, int iterations = 2)
    {
        if (rb == null)
            return;

        for (int i = 0; i < iterations; i++)
        {
            Collider2D overlap = Physics2D.OverlapCircle(rb.position, castRadius, obstacleMask);
            if (overlap == null)
                return;

            Vector2 position = rb.position;
            Vector2 separation = Vector2.zero;

            var hits = Physics2D.OverlapCircleAll(position, castRadius, obstacleMask);
            foreach (var hit in hits)
            {
                if (hit == null)
                    continue;

                Vector2 closest = hit.ClosestPoint(position);
                Vector2 away = position - closest;
                float dist = away.magnitude;
                if (dist < 1e-4f)
                    away = (position - (Vector2)hit.bounds.center).normalized;
                else
                    away /= dist;

                float push = Mathf.Max(castRadius - dist, Skin);
                separation += away * push;
            }

            if (separation.sqrMagnitude < 1e-6f)
                return;

            rb.MovePosition(position + separation);
            rb.linearVelocity = Vector2.zero;
        }
    }

    static bool IsBlocked(Vector2 origin, float radius, Vector2 dir, float distance, LayerMask mask)
    {
        return Physics2D.CircleCast(origin, radius, dir, distance, mask).collider != null;
    }

    static Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }
}
