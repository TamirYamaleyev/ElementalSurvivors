using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(PlayerStats))]
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Settings")]
    [SerializeField] private float iFrameDuration = .5f;
    [SerializeField] private MonoBehaviour statsProviderBehaviour;
    [SerializeField] private HitFlash hitFlash;
    [SerializeField] private PlayerHitSound hitSound;

    public event Action<float, float> OnHealthChanged;
    public event Action OnDied;

    private IPlayerStatsProvider _statsProvider;
    private float currentHealth;
    private float iFrameTimer;
    private float lastMaxHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => _statsProvider != null ? _statsProvider.Current.MaxHealth : 0f;

    void Awake()
    {
        if (statsProviderBehaviour is IPlayerStatsProvider provider)
            _statsProvider = provider;
        else
            _statsProvider = GetComponent<IPlayerStatsProvider>();
    }

    void OnEnable()
    {
        if (_statsProvider != null)
            _statsProvider.OnStatsChanged += HandleStatsChanged;
    }

    void OnDisable()
    {
        if (_statsProvider != null)
            _statsProvider.OnStatsChanged -= HandleStatsChanged;
    }

    void Start()
    {
        if (_statsProvider != null)
            InitializeFromSnapshot(_statsProvider.Current);
    }

    void Update()
    {
        if (iFrameTimer > 0f)
            iFrameTimer -= Time.deltaTime;
    }

    private void HandleStatsChanged(PlayerStatsSnapshot snapshot)
    {
        float newMax = snapshot.MaxHealth;
        if (newMax > lastMaxHealth)
            currentHealth += newMax - lastMaxHealth;

        lastMaxHealth = newMax;
        currentHealth = Mathf.Min(currentHealth, newMax);
        NotifyHealthChanged();
    }

    private void InitializeFromSnapshot(PlayerStatsSnapshot snapshot)
    {
        lastMaxHealth = snapshot.MaxHealth;
        currentHealth = snapshot.MaxHealth;
        NotifyHealthChanged();
    }

    public void TakeDamage(float amount)
    {
        if (iFrameTimer > 0f)
            return;

        currentHealth -= amount;
        NotifyHealthChanged();
        hitSound.PlayHitSound();
        //hitFlash.Play();

        iFrameTimer = iFrameDuration;

        if (currentHealth <= 0f)
            Die();
    }

    public void HealFractionOfMax(float fraction)
    {
        if (fraction <= 0f)
            return;

        float max = MaxHealth;
        if (max <= 0f)
            return;

        currentHealth = Mathf.Min(currentHealth + max * fraction, max);
        NotifyHealthChanged();
    }

    public void HealFlat(float amount)
    {
        if (amount <= 0f)
            return;

        float max = MaxHealth;
        if (max <= 0f)
            return;

        currentHealth = Mathf.Min(currentHealth + amount, max);
        NotifyHealthChanged();
    }

    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);
    }

    private void Die()
    {
        Debug.Log("Player died");
        OnDied?.Invoke();
        gameObject.SetActive(false);
    }
}
