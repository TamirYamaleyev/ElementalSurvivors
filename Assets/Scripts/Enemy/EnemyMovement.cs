using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Enemy))]
[RequireComponent(typeof(EnemyStats))]
[RequireComponent(typeof(EnemyStatusEffects))]
public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;

    private EnemyStats stats;
    private EnemyStatusEffects statusEffects;

    private Transform player;
    private Vector2 direction;
    private bool gameplayEnabled;

    private void Awake()
    {
        stats = GetComponent<EnemyStats>();
        statusEffects = GetComponent<EnemyStatusEffects>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
    }

    public void EnableGameplay()
    {
        gameplayEnabled = true;

        if (player == null)
            player = PlayerController.Instance;
    }

    public void DisableGameplay()
    {
        gameplayEnabled = false;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    private void FixedUpdate()
    {
        if (!gameplayEnabled || player == null)
            return;

        Vector2 baseDir = (player.position - transform.position).normalized;
        direction = statusEffects.ModifyDirection(baseDir);

        float speed = stats.MoveSpeed * statusEffects.SpeedMultiplier;
        rb.linearVelocity = direction * speed;
    }
}
