using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class DamageNumberView : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float driftPixels = 35f;

    private RectTransform rectTransform;
    private Coroutine playRoutine;

    private void Awake()
    {
        rectTransform = transform as RectTransform;

        if (text == null)
            text = GetComponent<TMP_Text>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Play(int damage, Color color, Vector2 anchoredPosition, float lifetime, Action<DamageNumberView> onComplete)
    {
        if (playRoutine != null)
            StopCoroutine(playRoutine);

        gameObject.SetActive(true);

        if (rectTransform != null)
            rectTransform.anchoredPosition = anchoredPosition;

        if (text != null)
        {
            text.text = damage.ToString();
            color.a = 1f;
            text.color = color;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        playRoutine = StartCoroutine(PlayRoutine(lifetime, onComplete));
    }

    private IEnumerator PlayRoutine(float lifetime, Action<DamageNumberView> onComplete)
    {
        Vector2 start = rectTransform != null ? rectTransform.anchoredPosition : Vector2.zero;
        Vector2 end = start + Vector2.up * driftPixels;
        float elapsed = 0f;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t = lifetime > 0f ? Mathf.Clamp01(elapsed / lifetime) : 1f;
            float alpha = 1f - t;

            if (rectTransform != null)
                rectTransform.anchoredPosition = Vector2.Lerp(start, end, t);

            if (canvasGroup != null)
                canvasGroup.alpha = alpha;
            else if (text != null)
            {
                Color c = text.color;
                c.a = alpha;
                text.color = c;
            }

            yield return null;
        }

        playRoutine = null;
        gameObject.SetActive(false);
        onComplete?.Invoke(this);
    }
}
