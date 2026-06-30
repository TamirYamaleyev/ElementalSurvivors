using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// One-shot hit flash on every enemy inside the explosion radius.
/// </summary>
[DisallowMultipleComponent]
public sealed class ReactionExplosionAreaFlash : MonoBehaviour, IReactionWorldVfx
{
    [SerializeField] private float effectRadius = 1.1f;
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private float flashScale = 0.55f;
    [SerializeField] private int sortingOrderOffset = 35;

    private static Sprite cachedFlashSprite;

    private readonly List<Enemy> scratchTargets = new();

    public void Initialize(ReactionVfxContext ctx)
    {
        if (ctx.Registry == null)
            return;

        ReactionAreaVfxUtility.CollectEnemiesInRadius(
            ctx.Registry,
            ctx.Center,
            effectRadius,
            scratchTargets);

        foreach (var enemy in scratchTargets)
            SpawnFlash(enemy);
    }

    private void SpawnFlash(Enemy enemy)
    {
        var go = new GameObject("ExplosionHitFlash");
        go.transform.position = enemy.transform.position + Vector3.up * 0.25f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetFlashSprite();
        sr.color = new Color(1f, 0.75f, 0.2f, 0.95f);
        go.transform.localScale = Vector3.one * flashScale;

        var body = enemy.GetComponentInChildren<SpriteRenderer>();
        if (body != null)
        {
            sr.sortingLayerID = body.sortingLayerID;
            sr.sortingOrder = body.sortingOrder + sortingOrderOffset;
        }

        go.AddComponent<ReactionExplosionFlashFade>().Begin(flashDuration);
    }

    private static Sprite GetFlashSprite()
    {
        if (cachedFlashSprite != null)
            return cachedFlashSprite;

        const int size = 32;
        var pixels = new Color32[size * size];
        var center = (size - 1) * 0.5f;

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / center;
                var alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(1f - dist) * 255f);
                pixels[y * size + x] = new Color32(255, 255, 255, alpha);
            }
        }

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };
        tex.SetPixels32(pixels);
        tex.Apply();

        cachedFlashSprite = Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        return cachedFlashSprite;
    }
}

/// <summary>Fades and destroys a one-shot explosion flash sprite.</summary>
public sealed class ReactionExplosionFlashFade : MonoBehaviour
{
    private float duration;
    private float elapsed;
    private SpriteRenderer spriteRenderer;
    private Color startColor;

    public void Begin(float lifetime)
    {
        duration = Mathf.Max(0.01f, lifetime);
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            startColor = spriteRenderer.color;
    }

    private void Update()
    {
        if (spriteRenderer == null)
        {
            Destroy(gameObject);
            return;
        }

        elapsed += Time.deltaTime;
        var t = Mathf.Clamp01(elapsed / duration);
        var c = startColor;
        c.a = startColor.a * (1f - t);
        spriteRenderer.color = c;

        transform.localScale = Vector3.one * Mathf.Lerp(0.55f, 0.9f, t);

        if (t >= 1f)
            Destroy(gameObject);
    }
}
