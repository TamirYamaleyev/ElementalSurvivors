using UnityEngine;

public class HealthCollectible : MonoBehaviour, ICollectible
{
    [SerializeField] [Range(0f, 1f)] private float healFractionOfMax = 0.1f;

    public void Collect(PlayerPickupFacade facade)
    {
        if (facade == null)
            return;

        facade.HealFractionOfMax(healFractionOfMax);
    }
}
