using UnityEngine;

public class PlayerBobbing : MonoBehaviour
{
    [SerializeField] private Transform bobTarget;

    [SerializeField] private float height = 0.08f;
    [SerializeField] private float frequency = 8f;
    [SerializeField] private float movementThreshold = 0.01f;

    private Vector3 startLocalPos;
    private Vector3 lastPosition;
    private float bobTime;

    private void Awake()
    {
        if (bobTarget == null)
            bobTarget = transform;

        startLocalPos = bobTarget.localPosition;
        lastPosition = transform.position;
    }

    private void Update()
    {
        Vector3 movement = transform.position - lastPosition;
        float speed = movement.magnitude / Time.deltaTime;

        lastPosition = transform.position;

        if (speed > movementThreshold)
        {
            bobTime += Time.deltaTime * frequency;
        }

        float bob = Mathf.Sin(bobTime) * height;

        bobTarget.localPosition = startLocalPos + Vector3.up * bob;
    }
}