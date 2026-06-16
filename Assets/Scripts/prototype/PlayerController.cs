using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent (typeof(PlayerHealth))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Rigidbody2D rb;

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 8f;

    private Vector2 moveInput;

    public static Transform Instance { get; private set; }

    void Awake()
    {
        Instance = transform;

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();
    }

    void FixedUpdate()
    {
        Move();    
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void Move()
    {
        rb.linearVelocity = moveInput.normalized * moveSpeed;
    }
}
