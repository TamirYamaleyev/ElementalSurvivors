using UnityEngine;

/// <summary>
/// Brief heat ring around the source at scorching wind trigger (sketch sphere).
/// </summary>
public sealed class ScorchingWindRingVisual : MonoBehaviour
{
    private static Sprite cachedRingSprite;
    private const int RingSpriteVersion = 1;

    private SpriteRenderer spriteRenderer;
    private float lifetime;
    private float elapsed;
    private Color startColor;

    public void Initialize(Vector2 center, float radius, float lifetime, int sortingOrder, int sortingLayerId)
    {
        this.lifetime = Mathf.Max(0.01f, lifetime);

        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetRingSprite();
        }

        spriteRenderer.sortingLayerID = sortingLayerId;
        spriteRenderer.sortingOrder = sortingOrder - 1;
        startColor = new Color(1f, 0.42f, 0.12f, 0.9f);
        spriteRenderer.color = startColor;

        transform.position = center;
        transform.localScale = Vector3.one * (radius * 2f);

        elapsed = 0f;
        Destroy(gameObject, this.lifetime);
    }

    private void Update()
    {
        if (spriteRenderer == null)
            return;

        elapsed += Time.deltaTime;
        var t = Mathf.Clamp01(elapsed / lifetime);
        var flash = t < 0.12f ? 1f : 1f - ((t - 0.12f) / 0.88f);
        var c = startColor;
        c.a = startColor.a * flash;
        spriteRenderer.color = c;
    }

    private static Sprite GetRingSprite()
    {
        if (cachedRingSprite != null)
            return cachedRingSprite;

        const int size = 64;
        const float pixelsPerUnit = 64f;
        const float center = (size - 1) * 0.5f;
        const float outerRadius = 29f;
        const float innerRadius = 25f;

        var pixels = new Color32[size * size];
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                var onRing = dist <= outerRadius && dist >= innerRadius;
                var alpha = onRing ? (byte)255 : (byte)0;
                pixels[y * size + x] = new Color32(255, 255, 255, alpha);
            }
        }

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };
        tex.SetPixels32(pixels);
        tex.Apply();

        cachedRingSprite = Sprite.Create(
            tex,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            pixelsPerUnit);

        return cachedRingSprite;
    }
}
