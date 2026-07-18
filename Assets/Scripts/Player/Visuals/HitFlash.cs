using System.Collections;
using UnityEngine;

public class HitFlash : MonoBehaviour
{
    [SerializeField] private GameObject flashObject;
    [SerializeField] private float flashDuration = 0.08f;

    private Coroutine flashRoutine;

    public void Play()
    {
        if (flashRoutine != null) 
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(Flash());
    }

    private IEnumerator Flash()
    {
        flashObject.SetActive(true);

        yield return new WaitForSeconds(flashDuration);

        flashObject.SetActive(false);
    }
}
