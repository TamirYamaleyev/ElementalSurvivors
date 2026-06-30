using UnityEngine;

public sealed class EnemyWorldHealthBar : MonoBehaviour
{
    [SerializeField] private EnemyHealth health;
    [SerializeField] private Transform fillTransform;
    [SerializeField] private float barWidth = 1.4f;
    [SerializeField] private Vector3 localOffset = new(0f, 1.15f, 0f);

    private Vector3 fillBaseScale = Vector3.one;
    private Transform followTarget;

    public static EnemyWorldHealthBar EnsureAttached(Enemy enemy, float width = 1.4f, float height = 0.1f)
    {
        if (enemy == null)
            return null;

        var health = enemy.GetComponent<EnemyHealth>();
        if (health == null)
            return null;

        var existing = enemy.GetComponentInChildren<EnemyWorldHealthBar>(true);
        if (existing != null)
            return existing;

        var barRoot = new GameObject("HealthBar");
        barRoot.transform.SetParent(enemy.transform, false);
        barRoot.transform.localPosition = new Vector3(0f, 1.15f, 0f);

        CreateBarSprite(barRoot.transform, "Background", new Vector3(width, height * 1.2f, 1f), Vector3.zero,
            new Color(0.1f, 0.1f, 0.1f, 0.85f), 50);

        var fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(barRoot.transform, false);
        fillGo.transform.localPosition = new Vector3(0f, 0f, -0.01f);
        fillGo.transform.localScale = new Vector3(width, height, 1f);
        CreateBarSprite(fillGo.transform, null, Vector3.one, Vector3.zero, new Color(0.85f, 0.15f, 0.15f, 1f), 51);

        var bar = barRoot.AddComponent<EnemyWorldHealthBar>();
        bar.health = health;
        bar.fillTransform = fillGo.transform;
        bar.barWidth = width;
        bar.localOffset = Vector3.zero;
        bar.fillBaseScale = fillGo.transform.localScale;
        return bar;
    }

    private static void CreateBarSprite(
        Transform parent,
        string name,
        Vector3 scale,
        Vector3 localPosition,
        Color color,
        int sortingOrder)
    {
        Transform target = parent;
        if (!string.IsNullOrEmpty(name))
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            target = go.transform;
        }

        target.localPosition = localPosition;
        target.localScale = scale;

        var sr = target.gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = CreateWhiteSprite();
        sr.color = color;
        sr.sortingOrder = sortingOrder;
    }

    private static Sprite _whiteSprite;

    private static Sprite CreateWhiteSprite()
    {
        if (_whiteSprite != null)
            return _whiteSprite;

        var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        _whiteSprite = Sprite.Create(tex, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        return _whiteSprite;
    }

    private void Awake()
    {
        if (health == null)
            health = GetComponentInParent<EnemyHealth>();

        followTarget = health != null ? health.transform : transform.parent;

        if (fillTransform != null)
            fillBaseScale = fillTransform.localScale;
    }

    private void OnEnable()
    {
        if (health == null)
            return;

        health.OnHealthChanged += HandleHealthChanged;
        HandleHealthChanged(health.CurrentHealth, health.MaxHealth);
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnHealthChanged -= HandleHealthChanged;
    }

    private void LateUpdate()
    {
        if (followTarget == null)
            return;

        transform.position = followTarget.position + localOffset;
    }

    private void HandleHealthChanged(float current, float max)
    {
        if (fillTransform == null)
            return;

        var fraction = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        var width = barWidth * fraction;
        fillTransform.localScale = new Vector3(fillBaseScale.x * width, fillBaseScale.y, fillBaseScale.z);
        fillTransform.localPosition = new Vector3(-barWidth * 0.5f + width * 0.5f, 0f, -0.01f);
    }
}
