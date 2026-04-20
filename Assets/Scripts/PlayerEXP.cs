using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerEXP : MonoBehaviour
{
    [Header("Progression")]
    [SerializeField] private int level = 1;
    [SerializeField] private float currentExp;

    [Tooltip("Baser EXP required for level 1 -> 2")]
    [SerializeField] private float baseExpToNext = 10;

    [Tooltip("Growth per level in %")]
    [SerializeField] private float growthFactor = 1.2f;

    private float expToNextCache;

    public event Action<float, float> OnExpChanged;
    public event Action<int> OnLevelUp;

    public int Level => level;
    public float CurrentExp => currentExp;
    public float ExpToNext => expToNextCache;

    void Awake()
    {
        RecalculateExpThreshold();
        OnExpChanged?.Invoke(currentExp, expToNextCache);
    }

    public void AddExp(float amount)
    {
        if (amount <= 0) return;
        
        currentExp += amount;

        while (currentExp >= expToNextCache)
        {
            currentExp -= expToNextCache;
            LevelUp();
        }

        OnExpChanged?.Invoke(currentExp, expToNextCache);
    }

    private void LevelUp()
    {
        level++;
        RecalculateExpThreshold();

        OnLevelUp?.Invoke(level);
        Debug.Log("Leveled Up");
    }

    private void RecalculateExpThreshold()
    {
        expToNextCache = MathF.Ceiling(baseExpToNext * MathF.Pow(growthFactor, level - 1));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<EXPOrb>(out var orb))
        {
            AddExp(orb.expToGive);
            Debug.Log($"+{orb.expToGive} EXP");
            Destroy(orb.gameObject);
        }
    }
}
