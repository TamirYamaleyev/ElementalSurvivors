using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private PlayerEXP playerExp;

    private Vector2 lastFacingDirection = Vector2.down;

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

        PlayerPickupRuntimeSetup.Ensure(transform);
    }

    void Update()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        playerAimDirection.SetMousePosition(mouseScreen);

        if (Keyboard.current.uKey.wasPressedThisFrame)
        {
            playerExp.DevLevelUp();
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void FixedUpdate()
    {
        Move();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        if (playerAimDirection != null)
            playerAimDirection.SetDirection(moveInput);

        if (moveInput.x > 0)
            visual.flipX = true;
        else if (moveInput.x < 0)
            visual.flipX = false;
    }

    private void Move()
    {
        float speed = _statsProvider != null ? _statsProvider.Current.MoveSpeed : 0f;
        rb.linearVelocity = moveInput.normalized * speed;
    }
}
