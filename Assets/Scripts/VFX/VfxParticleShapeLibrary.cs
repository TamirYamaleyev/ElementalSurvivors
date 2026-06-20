using UnityEngine;

/// <summary>
/// Procedural billboard sprites for elemental / reaction particle shapes.
/// </summary>
public static class VfxParticleShapeLibrary
{
    public enum Shape
    {
        Circle = 0,
        Triangle = 1,
        Square = 2,
        Hexagon = 3
    }

    private const int TextureSize = 64;
    private const float PixelsPerUnit = TextureSize;

    private static readonly Sprite[] CachedSprites = new Sprite[4];

    public static Sprite GetSprite(Shape shape)
    {
        var index = (int)shape;
        if (CachedSprites[index] != null)
            return CachedSprites[index];

        var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };

        FillShape(texture, shape);
        texture.Apply();

        CachedSprites[index] = Sprite.Create(
            texture,
            new Rect(0f, 0f, TextureSize, TextureSize),
            new Vector2(0.5f, 0.5f),
            PixelsPerUnit);

        return CachedSprites[index];
    }

    public static Shape GetElementShape(ElementalParticleBootstrap.PresetKind kind)
    {
        return kind switch
        {
            ElementalParticleBootstrap.PresetKind.Fire => Shape.Triangle,
            ElementalParticleBootstrap.PresetKind.Water => Shape.Circle,
            ElementalParticleBootstrap.PresetKind.Wind => Shape.Circle,
            ElementalParticleBootstrap.PresetKind.Earth => Shape.Square,
            ElementalParticleBootstrap.PresetKind.Lightning => Shape.Triangle,
            ElementalParticleBootstrap.PresetKind.BossRisingCone => Shape.Square,
            _ => Shape.Square
        };
    }

    public static Shape GetReactionShape(ReactionBurstParticleBootstrap.ReactionBurstKind kind)
    {
        return kind switch
        {
            ReactionBurstParticleBootstrap.ReactionBurstKind.Vaporize => Shape.Circle,
            ReactionBurstParticleBootstrap.ReactionBurstKind.Crystallize => Shape.Triangle,
            ReactionBurstParticleBootstrap.ReactionBurstKind.ScorchingWind => Shape.Hexagon,
            ReactionBurstParticleBootstrap.ReactionBurstKind.Explosion => Shape.Square,
            ReactionBurstParticleBootstrap.ReactionBurstKind.Growth => Shape.Circle,
            ReactionBurstParticleBootstrap.ReactionBurstKind.Hail => Shape.Square,
            ReactionBurstParticleBootstrap.ReactionBurstKind.Electrowetting => Shape.Circle,
            ReactionBurstParticleBootstrap.ReactionBurstKind.DustSandStorm => Shape.Hexagon,
            ReactionBurstParticleBootstrap.ReactionBurstKind.Magnetism => Shape.Circle,
            ReactionBurstParticleBootstrap.ReactionBurstKind.StaticCharge => Shape.Triangle,
            _ => Shape.Square
        };
    }

    private static void FillShape(Texture2D texture, Shape shape)
    {
        var pixels = new Color32[TextureSize * TextureSize];
        for (var y = 0; y < TextureSize; y++)
        {
            for (var x = 0; x < TextureSize; x++)
            {
                var alpha = SampleAlpha(shape, x + 0.5f, y + 0.5f);
                pixels[y * TextureSize + x] = new Color32(255, 255, 255, alpha);
            }
        }

        texture.SetPixels32(pixels);
    }

    private static byte SampleAlpha(Shape shape, float x, float y)
    {
        return shape switch
        {
            Shape.Circle => SampleCircle(x, y),
            Shape.Triangle => SampleTriangle(x, y),
            Shape.Square => SampleSquare(x, y),
            Shape.Hexagon => SampleHexagon(x, y),
            _ => 0
        };
    }

    private static byte SampleCircle(float x, float y)
    {
        const float center = TextureSize * 0.5f;
        const float radius = TextureSize * 0.44f;
        var distance = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center));
        return SoftEdge(distance, radius, 1.4f);
    }

    private static byte SampleSquare(float x, float y)
    {
        const float inset = TextureSize * 0.1f;
        var max = TextureSize - inset;
        if (x < inset || y < inset || x > max || y > max)
            return 0;

        var edgeDistance = Mathf.Min(x - inset, y - inset, max - x, max - y);
        return SoftEdge(edgeDistance, 0f, 1.2f, inverted: true);
    }

    private static byte SampleTriangle(float x, float y)
    {
        var a = new Vector2(TextureSize * 0.5f, TextureSize * 0.9f);
        var b = new Vector2(TextureSize * 0.08f, TextureSize * 0.1f);
        var c = new Vector2(TextureSize * 0.92f, TextureSize * 0.1f);
        var point = new Vector2(x, y);

        var inside = PointInTriangle(point, a, b, c);
        if (!inside)
            return 0;

        var edgeDistance = Mathf.Min(
            DistanceToSegment(point, a, b),
            DistanceToSegment(point, b, c),
            DistanceToSegment(point, c, a));

        return SoftEdge(edgeDistance, 0f, 1.2f, inverted: true);
    }

    private static byte SampleHexagon(float x, float y)
    {
        var center = new Vector2(TextureSize * 0.5f, TextureSize * 0.5f);
        var point = new Vector2(x, y);
        var vertices = GetHexagonVertices(center, TextureSize * 0.44f);

        if (!PointInConvexPolygon(point, vertices))
            return 0;

        var edgeDistance = float.MaxValue;
        for (var i = 0; i < vertices.Length; i++)
        {
            var next = vertices[(i + 1) % vertices.Length];
            edgeDistance = Mathf.Min(edgeDistance, DistanceToSegment(point, vertices[i], next));
        }

        return SoftEdge(edgeDistance, 0f, 1.2f, inverted: true);
    }

    private static Vector2[] GetHexagonVertices(Vector2 center, float radius)
    {
        var vertices = new Vector2[6];
        const float startAngle = Mathf.PI / 6f;
        for (var i = 0; i < 6; i++)
        {
            var angle = startAngle + i * (Mathf.PI / 3f);
            vertices[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        return vertices;
    }

    private static bool PointInConvexPolygon(Vector2 point, Vector2[] vertices)
    {
        var inside = false;
        for (int i = 0, j = vertices.Length - 1; i < vertices.Length; j = i++)
        {
            var vi = vertices[i];
            var vj = vertices[j];
            var intersects = vi.y > point.y != vj.y > point.y
                && point.x < (vj.x - vi.x) * (point.y - vi.y) / (vj.y - vi.y + Mathf.Epsilon) + vi.x;
            if (intersects)
                inside = !inside;
        }

        return inside;
    }

    private static byte SoftEdge(float value, float threshold, float softness, bool inverted = false)
    {
        var signed = inverted ? value - threshold : threshold - value;
        var alpha = Mathf.Clamp01(signed / softness + 1f);
        return (byte)Mathf.RoundToInt(alpha * 255f);
    }

    private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        var denominator = (b.y - c.y) * (a.x - c.x) + (c.x - b.x) * (a.y - c.y);
        if (Mathf.Approximately(denominator, 0f))
            return false;

        var alpha = ((b.y - c.y) * (p.x - c.x) + (c.x - b.x) * (p.y - c.y)) / denominator;
        var beta = ((c.y - a.y) * (p.x - c.x) + (a.x - c.x) * (p.y - c.y)) / denominator;
        var gamma = 1f - alpha - beta;
        return alpha >= 0f && beta >= 0f && gamma >= 0f;
    }

    private static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        var ab = b - a;
        var lengthSq = Vector2.Dot(ab, ab);
        if (lengthSq <= Mathf.Epsilon)
            return Vector2.Distance(p, a);

        var t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / lengthSq);
        var projection = a + ab * t;
        return Vector2.Distance(p, projection);
    }
}
