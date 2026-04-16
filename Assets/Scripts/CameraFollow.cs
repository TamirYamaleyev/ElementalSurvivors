using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float smoothTime = 0.15f;

    private Transform player;
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        player = PlayerController.Instance;
    }

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 targetPos = new Vector3(player.position.x, player.position.y, transform.position.z);

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
    }
}
