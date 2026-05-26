using UnityEngine;

public class PlayerPickupFacade : MonoBehaviour
{
    [SerializeField] private PlayerEXP playerExp;
    [SerializeField] private PlayerHealth playerHealth;

    void Awake()
    {
        if (playerExp == null)
            playerExp = GetComponent<PlayerEXP>();
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();
    }

    public void AddExp(float amount)
    {
        if (playerExp == null)
            return;

        playerExp.AddExp(amount);
    }

    public void HealFractionOfMax(float fraction)
    {
        if (playerHealth == null)
            return;

        playerHealth.HealFractionOfMax(fraction);
    }
}
