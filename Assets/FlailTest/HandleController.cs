using UnityEngine;
using UnityEngine.InputSystem;

public class HandleController : MonoBehaviour
{
    [SerializeField] private InputHandler input;
    [SerializeField] private Rigidbody2D handleRb;

    [SerializeField] private float handleSpeed = 10f;

    private Vector2 targetPosition;

    void Update()
    {
        if (input.IsHoldingLMB)
            targetPosition = Vector2.MoveTowards(handleRb.position, input.WorldPosition, handleSpeed * Time.fixedDeltaTime);
    }

    void FixedUpdate()
    {
        handleRb.MovePosition(targetPosition);
    }
}
