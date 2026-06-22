using UnityEngine;

/// <summary>
/// Single straight scorching ray stretched from the ring edge along a direction.
/// </summary>
public sealed class ScorchingRayVisual : MonoBehaviour
{
    private static Sprite cachedBeamSprite;
    private static int cachedSpriteVersion;
    private const int BeamSpriteVersion = 3;

    [SerializeField] private float thickness = 0.4f;
    [SerializeField] private float shootDuration = 0.07f;

    private SpriteRenderer spriteRenderer;
    private float lifetime;
    private float targetLength;
    private float spriteHeight = 1f;
    private float elapsed;
    private Color startColor = Color.white;

    public void Initialize(
        Vector2 origin,
        Vector2 direction,
        float length,
        float lifetime,
        int sortingOrder,
        int sortingLayerId)
    {
        this.lifetime = Mathf.Max(0.01f, lifetime);
        targetLength = length;
        elapsed = 0f;

        var dir = direction.sqrMagnitude > 1e-6f ? direction.normalized : Vector2.up;

        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBeamSprite();
        }

        spriteRenderer.sortingLayerID = sortingLayerId;
        spriteRenderer.sortingOrder = sortingOrder;
        startColor = new Color(1f, 0.28f, 0.06f, 1f);
        spriteRenderer.color = startColor;

        transform.position = origin;
        transform.rotation = Quaternion.FromToRotation(Vector3.up, new Vector3(dir.x, dir.y, 0f));

        spriteHeight = spriteRenderer.sprite != null ? spriteRenderer.sprite.bounds.size.y : 1f;
        if (spriteHeight < 1e-4f)
            spriteHeight = 1f;

        transform.localScale = new Vector3(thickness, 0f, 1f);
        Destroy(gameObject, this.lifetime);
    }

    private void Update()
    {
        if (spriteRenderer == null || lifetime <= 0f)
            return;

        elapsed += Time.deltaTime;
        var t = Mathf.Clamp01(elapsed / lifetime);

        var shootT = shootDuration > 0f ? Mathf.Clamp01(elapsed / shootDuration) : 1f;
        var easedShoot = 1f - (1f - shootT) * (1f - shootT);
        var currentLength = targetLength * easedShoot;
        transform.localScale = new Vector3(thickness, currentLength / spriteHeight, 1f);

        var fadeStart = 0.55f;
        var alpha = t < fadeStart ? 1f : 1f - ((t - fadeStart) / (1f - fadeStart));
        var c = startColor;
        c.a = alpha;
        spriteRenderer.color = c;
    }

    private static Sprite GetBeamSprite()
    {
        if (cachedBeamSprite != null && cachedSpriteVersion == BeamSpriteVersion)
            return cachedBeamSprite;

        cachedBeamSprite = null;

        const int width = 20;
        const int height = 64;
        const float pixelsPerUnit = 64f;

        var pixels = new Color32[width * height];
        var centerX = (width - 1) * 0.5f;

        for (var y = 0; y < height; y++)
        {
            var vertical = y / (height - 1f);
            var rootFade = vertical < 0.04f
                ? Mathf.InverseLerp(0f, 0.04f, vertical)
                : 1f;
            var tipFade = vertical > 0.9f
                ? Mathf.InverseLerp(1f, 0.9f, vertical)
                : 1f;
            var verticalFade = rootFade * tipFade;

            for (var x = 0; x < width; x++)
            {
                var dist = Mathf.Abs(x - centerX) / (width * 0.5f);
                var core = 1f - Mathf.Clamp01(dist);
                core = core > 0.35f ? 1f : core / 0.35f;
                var rgb = Color32.Lerp(
                    new Color32(255, 45, 8, 255),
                    new Color32(255, 210, 120, 255),
                    core);
                var alpha = (byte)Mathf.RoundToInt(verticalFade * 255f);
                rgb.a = alpha;
                pixels[y * width + x] = rgb;
            }
        }

        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };
        tex.SetPixels32(pixels);
        tex.Apply();

        cachedBeamSprite = Sprite.Create(
            tex,
            new Rect(0f, 0f, width, height),
            new Vector2(0.5f, 0f),
            pixelsPerUnit);

        cachedSpriteVersion = BeamSpriteVersion;
        return cachedBeamSprite;
    }
}
