using UnityEngine;

public class ChaingLightningVisual : MonoBehaviour
{
    [SerializeField] AudioClip sfx;

    [SerializeField] private Sprite[] frames;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private float offset = 0.4f;
    [SerializeField] private float thickness = 1f;

    [SerializeField] private float frameRate = 60f;

    private float frameTimer;

    public void SwapSpriteSheet(Sprite[] newSprites)
    {
        frames = newSprites;
    }

    private void Awake()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        AudioManager.Instance.PlaySfx(sfx);
    }

    public void Initialize(Vector2 start, Vector2 end, Sprite sprite, float lifetime, float endpointInset = -1f)
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        Vector2 delta = end - start;
        if (delta.sqrMagnitude < 1e-6f)
            delta = Vector2.up;

        Vector2 dir = delta.normalized;

        var inset = endpointInset >= 0f ? endpointInset : offset;
        start += dir * inset;
        end -= dir * inset;

        transform.position = (start + end) * 0.5f;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Sprite activeSprite = PickSprite(sprite);
        if (sr != null && activeSprite != null)
            sr.sprite = activeSprite;

        float distance = Vector2.Distance(start, end);
        float spriteHeight = activeSprite != null ? activeSprite.bounds.size.y : 1f;
        if (spriteHeight < 1e-4f)
            spriteHeight = 1f;

        transform.localScale = new Vector3(thickness, distance / spriteHeight, 1f);

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (sr == null || frames == null || frames.Length == 0)
            return;

        frameTimer += Time.deltaTime;

        if (frameTimer >= 1f / frameRate)
        {
            frameTimer = 0f;

            Sprite frame = frames[Random.Range(0, frames.Length)];
            if (frame != null)
                sr.sprite = frame;
        }
    }

    Sprite PickSprite(Sprite fallback)
    {
        if (frames != null && frames.Length > 0)
        {
            for (int attempt = 0; attempt < frames.Length; attempt++)
            {
                Sprite frame = frames[Random.Range(0, frames.Length)];
                if (frame != null)
                    return frame;
            }
        }

        return fallback;
    }
}
