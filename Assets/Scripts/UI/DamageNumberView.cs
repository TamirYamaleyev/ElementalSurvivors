using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class DamageNumberView : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float driftWorldUnits = 0.35f;

    private Transform cachedTransform;
    private Coroutine playRoutine;

    private void Awake()
    {
        cachedTransform = transform;

        if (text == null)
            text = GetComponent<TMP_Text>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        TmpFontUtility.EnsureAssigned(text);
    }

    public void Play(int damage, Color color, Vector3 worldPosition, float lifetime, Action<DamageNumberView> onComplete)
    {
        if (playRoutine != null)
            StopCoroutine(playRoutine);

        gameObject.SetActive(true);

        if (cachedTransform != null)
            cachedTransform.position = worldPosition;

        if (text != null)
        {
            text.text = damage.ToString();
            color.a = 1f;
            text.color = color;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        playRoutine = StartCoroutine(PlayRoutine(worldPosition, lifetime, onComplete));
    }

    private IEnumerator PlayRoutine(Vector3 startWorldPosition, float lifetime, Action<DamageNumberView> onComplete)
    {
        Vector3 endWorldPosition = startWorldPosition + Vector3.up * driftWorldUnits;
        float elapsed = 0f;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t = lifetime > 0f ? Mathf.Clamp01(elapsed / lifetime) : 1f;
            float alpha = 1f - t;

            if (cachedTransform != null)
                cachedTransform.position = Vector3.Lerp(startWorldPosition, endWorldPosition, t);

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
