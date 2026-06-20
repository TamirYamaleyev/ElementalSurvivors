using UnityEngine;

/// <summary>
/// Single straight scorching ray stretched from origin along a direction.
/// </summary>
public sealed class ScorchingRayVisual : MonoBehaviour
{
    private static readonly Color CoreWhite = new(1f, 0.97f, 0.88f, 1f);
    private static readonly Color EdgeOrange = new(1f, 0.2f, 0.08f, 1f);

    private static Sprite cachedBeamSprite;

    [SerializeField] private float thickness = 0.13f;

    private SpriteRenderer spriteRenderer;
    private float lifetime;
    private float elapsed;
    private Color startColor = Color.white;

    public void Initialize(Vector2 origin, float angleDeg, float length, float lifetime, int sortingOrder, int sortingLayerId)
    {
        this.lifetime = Mathf.Max(0.01f, lifetime);
        elapsed = 0f;

        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBeamSprite();
        }

        spriteRenderer.sortingLayerID = sortingLayerId;
        spriteRenderer.sortingOrder = sortingOrder;
        startColor = Color.Lerp(CoreWhite, EdgeOrange, Random.Range(0.15f, 0.45f));
        spriteRenderer.color = startColor;

        transform.position = origin;
        transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);

        var spriteHeight = spriteRenderer.sprite != null ? spriteRenderer.sprite.bounds.size.y : 1f;
        if (spriteHeight < 1e-4f)
            spriteHeight = 1f;

        transform.localScale = new Vector3(thickness, length / spriteHeight, 1f);
        Destroy(gameObject, this.lifetime);
    }

    private void Update()
    {
        if (spriteRenderer == null || lifetime <= 0f)
            return;

        elapsed += Time.deltaTime;
        var t = Mathf.Clamp01(elapsed / lifetime);
        var alpha = 1f - t;
        var c = startColor;
        c.a = alpha;
        spriteRenderer.color = c;
    }

    private static Sprite GetBeamSprite()
    {
        if (cachedBeamSprite != null)
            return cachedBeamSprite;

        const int width = 12;
        const int height = 64;
        const float pixelsPerUnit = 64f;

        var pixels = new Color32[width * height];
        var centerX = (width - 1) * 0.5f;

        for (var y = 0; y < height; y++)
        {
            var vertical = y / (height - 1f);
            var tipFade = vertical < 0.08f || vertical > 0.92f
                ? Mathf.InverseLerp(0f, 0.08f, vertical < 0.08f ? vertical : 1f - vertical)
                : 1f;

            for (var x = 0; x < width; x++)
            {
                var dist = Mathf.Abs(x - centerX) / (width * 0.5f);
                var core = 1f - Mathf.Clamp01(dist);
                var rgb = Color32.Lerp(
                    new Color32(255, 51, 20, 255),
                    new Color32(255, 247, 224, 255),
                    core);
                var alpha = (byte)Mathf.RoundToInt(Mathf.Lerp(0.65f, 1f, core) * tipFade * 255f);
                rgb.a = alpha;
                pixels[y * width + x] = rgb;
            }
        }

        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
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

        return cachedBeamSprite;
    }
}
