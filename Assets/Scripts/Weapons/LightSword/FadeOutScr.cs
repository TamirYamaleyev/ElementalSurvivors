using System.Collections;
using UnityEngine;

public class FadeOutScr : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private float fadeDuration = 0.5f;

    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        Color color = sr.color;
        color.a = 0f;
        sr.color = color;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            color.a = Mathf.Clamp01(elapsed / fadeDuration);
            sr.color = color;

            yield return null;
        }

        color.a = 1f;
        sr.color = color;
    }
}
