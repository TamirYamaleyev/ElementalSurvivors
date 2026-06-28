using UnityEngine;

[RequireComponent(typeof(EnemyAI))]
[RequireComponent(typeof(BossAttackController))]
public sealed class BossAI : MonoBehaviour, IEnemyPoolReset, IEnemyPoolReset
{
    [Header("Movement")]
    [SerializeField] private float preferredDistance = 6f;
    [SerializeField] private float distanceTolerance = 1f;
    [SerializeField] private float strafeDirectionChangeInterval = 2.5f;

    private EnemyAI ai;
    private BossAttackController attack;
    private float strafeTimer;
    private int strafeSign = 1;

    private void Awake()
    {
        ai = GetComponent<EnemyAI>();
        attack = GetComponent<BossAttackController>();
    }

    private void OnEnable()
    {
        ai.EnsureInitialized();
        ai.SetDistanceMaintenance(preferredDistance, distanceTolerance, true);
        attack.AttackStarted += HandleAttackStarted;
        attack.AttackFinished += HandleAttackFinished;
        PickStrafeDirection();
    }

    private void OnDisable()
    {
        if (attack != null)
        {
            attack.AttackStarted -= HandleAttackStarted;
            attack.AttackFinished -= HandleAttackFinished;
        }
    }

    public void ResetForPool()
    {
        strafeTimer = 0f;
        strafeSign = 1;
        ai?.SetMovementOverride(true);
        ai?.SetDistanceMaintenance(preferredDistance, distanceTolerance, true);
        ai?.SetStrafeDirection(Vector2.zero);
    }

    private void Update()
    {
        if (ai == null || attack == null || attack.IsAttacking)
            return;

        strafeTimer -= Time.deltaTime;
        if (strafeTimer <= 0f)
            PickStrafeDirection();

        ApplyStrafe();
    }

    private void HandleAttackStarted()
    {
        ai.SetMovementOverride(false);
        ai.SetStrafeDirection(Vector2.zero);
    }

    private void HandleAttackFinished()
    {
        ai.SetMovementOverride(true);
    }

    private void PickStrafeDirection()
    {
        strafeSign = Random.value > 0.5f ? 1 : -1;
        strafeTimer = strafeDirectionChangeInterval;
    }

    private void ApplyStrafe()
    {
        if (PlayerController.Instance == null)
            return;

        var toPlayer = ((Vector2)PlayerController.Instance.transform.position - (Vector2)transform.position).normalized;
        var tangent = new Vector2(-toPlayer.y, toPlayer.x) * strafeSign;
        ai.SetStrafeDirection(tangent);
    }
}
