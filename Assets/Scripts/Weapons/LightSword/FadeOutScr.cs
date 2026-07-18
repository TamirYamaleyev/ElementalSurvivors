using System.Collections;
using UnityEngine;

public class FadeOutScr : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private float fadeDuration = 0.5f;

    void Start()
    {
        StartCoroutine(FadeOut());
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

    private IEnumerator FadeOut()
    {
        Color color = sr.color;
        color.a = 1f;
        sr.color = color;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            color.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            sr.color = color;

            yield return null;
        }

        color.a = 0f;
        sr.color = color;
    }
}
