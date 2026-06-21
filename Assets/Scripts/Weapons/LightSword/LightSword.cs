using System.Collections;
using UnityEngine;

public class LightSword : MonoBehaviour
{
    [SerializeField] private float rotationDuration = 0.5f;

    void Start()
    {
        Rotate90(true);
    }

    private void Rotate90(bool isClockwise)
    {
        if (isClockwise)
            StartCoroutine(RotateRoutine(90f));
        else
            StartCoroutine(RotateRoutine(-90f));
    }

    private IEnumerator RotateRoutine(float angle)
    {
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(0f, 0f, angle);

        float elapsed = 0f;

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / rotationDuration;
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);

            yield return null;
        }

        transform.rotation = targetRotation;
    }
}
