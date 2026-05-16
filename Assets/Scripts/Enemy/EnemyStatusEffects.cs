using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyStatusEffects : MonoBehaviour, IStatusEffectTarget
{
    [SerializeField] private float dotTickInterval = 0.5f;

    private EnemyHealth health;
    private IDamageable damageable;

    private float slowMultiplier = 0.5f;
    private float slowTimer;
    private float fearTimer;

    private float dotDuration;
    private float dotTickTimer;
    private float dotDamage = 1f;

    private bool gameplayEnabled;

    public float SpeedMultiplier
    {
        get
        {
            if (slowTimer > 0f)
                return slowMultiplier;

            return 1f;
        }
    }

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        damageable = health;
    }

    public void EnableGameplay()
    {
        gameplayEnabled = true;
    }

    public void DisableGameplay()
    {
        gameplayEnabled = false;
        ResetEffects();
    }

    public Vector2 ModifyDirection(Vector2 baseDirection)
    {
        if (fearTimer > 0f)
        {
            fearTimer -= Time.fixedDeltaTime;
            return -baseDirection;
        }

        return baseDirection;
    }

    public void ApplyFear(float duration)
    {
        fearTimer = duration;
    }

    public void ApplySlow(float duration, float multiplier)
    {
        slowTimer = duration;
        slowMultiplier = multiplier;
    }

    public void ApplyDoT(float duration, float damagePerTick)
    {
        dotDuration = duration;
        dotDamage = damagePerTick;
        dotTickTimer = 0f;
    }

    private void Update()
    {
        if (!gameplayEnabled)
            return;

        if (slowTimer > 0f)
            slowTimer -= Time.deltaTime;

        if (dotDuration > 0f)
            TickDoT();
    }

    private void TickDoT()
    {
        dotDuration -= Time.deltaTime;
        dotTickTimer += Time.deltaTime;

        if (dotTickTimer >= dotTickInterval)
        {
            damageable.TakeDamage(dotDamage);
            dotTickTimer = 0f;
        }
    }

    private void ResetEffects()
    {
        slowTimer = 0f;
        fearTimer = 0f;
        dotDuration = 0f;
        dotTickTimer = 0f;
    }
}
