using UnityEngine;

/// <summary>
/// Gen 1–3 Pokémon-style paralysis: yellow jagged bolts flanking the target, flickering on/off.
/// </summary>
[DisallowMultipleComponent]
public sealed class ReactionStaticChargeParalysisVisual : MonoBehaviour, IReactionWorldVfx
{
    private static readonly Color BoltYellow = new(1f, 0.93f, 0.08f, 1f);

    [SerializeField] private float horizontalOffset = 0.38f;
    [SerializeField] private float verticalOffset = 0.12f;
    [SerializeField] private float boltWorldScale = 0.55f;
    [SerializeField] private float flickerInterval = 0.09f;
    [SerializeField] private int sortingOrder = 42;

    private SpriteRenderer leftRenderer;
    private SpriteRenderer rightRenderer;
    private Sprite[] leftFrames;
    private Sprite[] rightFrames;

    private Vector3 center;
    private Enemy sourceEnemy;
    private float flickerTimer;
    private int frameIndex;
    private bool boltsVisible = true;

    private void Awake()
    {
        leftFrames = CreateBoltFrameSet(mirrorX: false);
        rightFrames = CreateBoltFrameSet(mirrorX: true);

        leftRenderer = CreateBoltRenderer("LeftParalysisBolt", leftFrames[0]);
        rightRenderer = CreateBoltRenderer("RightParalysisBolt", rightFrames[0]);
    }

    public void Initialize(ReactionVfxContext ctx)
    {
        center = ctx.Center;
        sourceEnemy = ctx.SourceEnemy;
        ApplyPositions();
    }

    private void Update()
    {
        if (sourceEnemy != null)
            center = sourceEnemy.transform.position + Vector3.up * 0.25f;

        ApplyPositions();

        flickerTimer += Time.deltaTime;
        if (flickerTimer < flickerInterval)
            return;

        flickerTimer = 0f;
        boltsVisible = !boltsVisible;

        if (boltsVisible)
        {
            frameIndex = (frameIndex + 1) % leftFrames.Length;
            leftRenderer.sprite = leftFrames[frameIndex];
            rightRenderer.sprite = rightFrames[frameIndex];
        }

        leftRenderer.enabled = boltsVisible;
        rightRenderer.enabled = boltsVisible;
    }

    private void ApplyPositions()
    {
        if (leftRenderer == null || rightRenderer == null)
            return;

        leftRenderer.transform.position = center + new Vector3(-horizontalOffset, verticalOffset, 0f);
        rightRenderer.transform.position = center + new Vector3(horizontalOffset, verticalOffset, 0f);
    }

    private SpriteRenderer CreateBoltRenderer(string objectName, Sprite sprite)
    {
        var go = new GameObject(objectName);
        go.transform.SetParent(transform, false);
        go.transform.localScale = Vector3.one * boltWorldScale;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = BoltYellow;
        sr.sortingOrder = sortingOrder;

        return sr;
    }

    private static Sprite[] CreateBoltFrameSet(bool mirrorX)
    {
        return new[]
        {
            CreateBoltSprite(mirrorX, variant: 0),
            CreateBoltSprite(mirrorX, variant: 1),
            CreateBoltSprite(mirrorX, variant: 2),
        };
    }

    private static Sprite CreateBoltSprite(bool mirrorX, int variant)
    {
        const int width = 14;
        const int height = 28;
        const float pixelsPerUnit = 14f;

        var pixels = new Color32[width * height];
        for (var i = 0; i < pixels.Length; i++)
            pixels[i] = new Color32(0, 0, 0, 0);

        var points = BuildBoltPoints(variant);
        for (var i = 0; i < points.Length - 1; i++)
            DrawThickLine(pixels, width, height, points[i], points[i + 1], mirrorX, thickness: 2);

        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        tex.SetPixels32(pixels);
        tex.Apply(false, true);

        return Sprite.Create(
            tex,
            new Rect(0f, 0f, width, height),
            new Vector2(0.5f, 0.5f),
            pixelsPerUnit);
    }

    private static Vector2[] BuildBoltPoints(int variant)
    {
        return variant switch
        {
            1 => new[]
            {
                new Vector2(0.18f, 0.96f),
                new Vector2(0.82f, 0.68f),
                new Vector2(0.14f, 0.38f),
                new Vector2(0.86f, 0.08f),
            },
            2 => new[]
            {
                new Vector2(0.24f, 0.94f),
                new Vector2(0.74f, 0.72f),
                new Vector2(0.22f, 0.42f),
                new Vector2(0.78f, 0.12f),
            },
            _ => new[]
            {
                new Vector2(0.2f, 0.98f),
                new Vector2(0.78f, 0.66f),
                new Vector2(0.18f, 0.34f),
                new Vector2(0.82f, 0.04f),
            },
        };
    }

    private static void DrawThickLine(
        Color32[] pixels,
        int width,
        int height,
        Vector2 fromNorm,
        Vector2 toNorm,
        bool mirrorX,
        int thickness)
    {
        var from = NormToPixel(fromNorm, width, height, mirrorX);
        var to = NormToPixel(toNorm, width, height, mirrorX);
        DrawLine(pixels, width, height, from, to, thickness);
    }

    private static Vector2Int NormToPixel(Vector2 norm, int width, int height, bool mirrorX)
    {
        var x = Mathf.RoundToInt(norm.x * (width - 1));
        if (mirrorX)
            x = width - 1 - x;

        var y = Mathf.RoundToInt(norm.y * (height - 1));
        return new Vector2Int(x, y);
    }

    private static void DrawLine(
        Color32[] pixels,
        int width,
        int height,
        Vector2Int from,
        Vector2Int to,
        int thickness)
    {
        var dx = Mathf.Abs(to.x - from.x);
        var dy = Mathf.Abs(to.y - from.y);
        var sx = from.x < to.x ? 1 : -1;
        var sy = from.y < to.y ? 1 : -1;
        var err = dx - dy;
        var x = from.x;
        var y = from.y;

        while (true)
        {
            StampPixelBlock(pixels, width, height, x, y, thickness);

            if (x == to.x && y == to.y)
                break;

            var e2 = err * 2;
            if (e2 > -dy)
            {
                err -= dy;
                x += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y += sy;
            }
        }
    }

    private static void StampPixelBlock(Color32[] pixels, int width, int height, int cx, int cy, int thickness)
    {
        var half = thickness / 2;
        for (var ox = -half; ox <= half; ox++)
        {
            for (var oy = -half; oy <= half; oy++)
            {
                var px = cx + ox;
                var py = cy + oy;
                if (px < 0 || px >= width || py < 0 || py >= height)
                    continue;

                pixels[py * width + px] = new Color32(255, 237, 20, 255);
            }
        }
    }
}
