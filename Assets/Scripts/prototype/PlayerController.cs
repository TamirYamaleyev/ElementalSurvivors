using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerAimDirection playerAimDirection;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private MonoBehaviour statsProviderBehaviour;

    private IPlayerStatsProvider _statsProvider;
    private Vector2 moveInput;

    public static Transform Instance { get; private set; }

    void Awake()
    {
        Instance = transform;

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        if (statsProviderBehaviour is IPlayerStatsProvider provider)
            _statsProvider = provider;
        else
            _statsProvider = GetComponent<IPlayerStatsProvider>();
    }

    void FixedUpdate()
    {
        Move();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        playerAimDirection.SetDirection(moveInput);

    }

    private void Move()
    {
        float speed = _statsProvider != null ? _statsProvider.Current.MoveSpeed : 0f;
        rb.linearVelocity = moveInput.normalized * speed;
    }
}
