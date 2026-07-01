using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class DamageNumberView : MonoBehaviour
{
    private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");
    private static readonly int UnderlayColorId = Shader.PropertyToID("_UnderlayColor");
    private static readonly int UnderlayDilateId = Shader.PropertyToID("_UnderlayDilate");

    [SerializeField] private TMP_Text text;
    [SerializeField] private TMP_Text outlineText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private float outlineWidth = 0.5f;
    [SerializeField] private float driftWorldUnits = 0.35f;

    private Transform cachedTransform;
    private Coroutine playRoutine;
    private Material outlineMaterialInstance;

    private void Awake()
    {
        cachedTransform = transform;

        if (text == null)
            text = GetComponent<TMP_Text>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        EnsureOutlineLayer();
        ApplyMaterials();
    }

    private void OnDestroy()
    {
        if (outlineMaterialInstance != null)
            Destroy(outlineMaterialInstance);
    }

    private void OnEnable()
    {
        if (text == null)
            text = GetComponent<TMP_Text>();

        EnsureOutlineLayer();
        ApplyMaterials();
    }

    private void EnsureOutlineLayer()
    {
        if (outlineText != null || text == null)
            return;

        var outlineGo = new GameObject("Outline", typeof(RectTransform));
        var outlineRect = outlineGo.GetComponent<RectTransform>();
        outlineRect.SetParent(transform, false);
        outlineRect.anchorMin = new Vector2(0.5f, 0.5f);
        outlineRect.anchorMax = new Vector2(0.5f, 0.5f);
        outlineRect.pivot = new Vector2(0.5f, 0.5f);
        outlineRect.anchoredPosition = Vector2.zero;
        outlineRect.sizeDelta = text.rectTransform.sizeDelta;
        outlineRect.localScale = Vector3.one;
        outlineGo.transform.SetAsFirstSibling();

        outlineText = outlineGo.AddComponent<TextMeshProUGUI>();
        CopyTextLayout(text, outlineText);
        outlineText.raycastTarget = false;
        outlineText.color = Color.black;
    }

    private static void CopyTextLayout(TMP_Text source, TMP_Text dest)
    {
        TmpFontUtility.EnsureAssigned(source, preserveSharedMaterial: true);
        TmpFontUtility.EnsureAssigned(dest, preserveSharedMaterial: true);

        dest.font = source.font;
        dest.fontSize = source.fontSize;
        dest.fontStyle = source.fontStyle;
        dest.alignment = source.alignment;
        dest.enableAutoSizing = source.enableAutoSizing;
        dest.fontSizeMin = source.fontSizeMin;
        dest.fontSizeMax = source.fontSizeMax;
        dest.characterSpacing = source.characterSpacing;
        dest.lineSpacing = source.lineSpacing;
        dest.margin = source.margin;
        dest.textWrappingMode = source.textWrappingMode;
        dest.overflowMode = source.overflowMode;
    }

    private void ApplyMaterials()
    {
        TmpFontUtility.EnsureAssigned(text, preserveSharedMaterial: true);

        if (text != null && text.font != null)
            text.fontSharedMaterial = text.font.material;

        if (outlineText == null || outlineMaterial == null)
            return;

        if (outlineMaterialInstance == null)
            outlineMaterialInstance = new Material(outlineMaterial);

        outlineMaterialInstance.EnableKeyword("OUTLINE_ON");
        outlineMaterialInstance.EnableKeyword("UNDERLAY_ON");
        outlineMaterialInstance.SetColor(OutlineColorId, Color.black);
        outlineMaterialInstance.SetFloat(OutlineWidthId, outlineWidth);
        outlineMaterialInstance.SetColor(UnderlayColorId, new Color(0f, 0f, 0f, 0.9f));
        outlineMaterialInstance.SetFloat(UnderlayDilateId, 0.28f);

        outlineText.font = text != null ? text.font : outlineText.font;
        outlineText.fontSharedMaterial = outlineMaterialInstance;
    }

    public void Play(int damage, Color color, Vector3 worldPosition, float lifetime, Action<DamageNumberView> onComplete)
    {
        if (playRoutine != null)
            StopCoroutine(playRoutine);

        gameObject.SetActive(true);
        EnsureOutlineLayer();
        ApplyMaterials();

        if (cachedTransform != null)
            cachedTransform.position = worldPosition;

        var damageText = damage.ToString();

        if (outlineText != null)
        {
            outlineText.text = damageText;
            outlineText.color = Color.black;
            outlineText.ForceMeshUpdate(true);
        }

        if (text != null)
        {
            text.text = damageText;
            color.a = 1f;
            text.color = color;
            text.ForceMeshUpdate(true);
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        playRoutine = StartCoroutine(PlayRoutine(worldPosition, lifetime, onComplete));
    }

    private IEnumerator PlayRoutine(Vector3 startWorldPosition, float lifetime, Action<DamageNumberView> onComplete)
    {
        Vector3 endWorldPosition = startWorldPosition + Vector3.up * driftWorldUnits;
        float elapsed = 0f;
        var fillColor = text != null ? text.color : Color.white;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t = lifetime > 0f ? Mathf.Clamp01(elapsed / lifetime) : 1f;
            float alpha = 1f - t;

            if (cachedTransform != null)
                cachedTransform.position = Vector3.Lerp(startWorldPosition, endWorldPosition, t);

            if (text != null)
            {
                var c = fillColor;
                c.a = alpha;
                text.color = c;
            }

            if (outlineText != null)
            {
                var outlineColor = Color.black;
                outlineColor.a = alpha;
                outlineText.color = outlineColor;
            }

            yield return null;
        }

        playRoutine = null;
        gameObject.SetActive(false);
        onComplete?.Invoke(this);
    }
}
