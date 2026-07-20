using UnityEngine;

public class CollectibleBobbing : MonoBehaviour
{
    [SerializeField] private float height = 0.25f;
    [SerializeField] private float frequency = 2f;

    private Vector3 startLocalPos;
    private float offset;

    void Awake()
    {
        startLocalPos = transform.localPosition;
        offset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        float bob = (Mathf.Sin(Time.time * frequency + offset) + 1f) * 0.5f;

        transform.localPosition = startLocalPos + Vector3.up * (bob * height);
    }
}
