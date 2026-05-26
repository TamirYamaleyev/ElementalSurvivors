using UnityEngine;

public class HealthCollectible : MonoBehaviour, ICollectible
{
    [SerializeField] private float healAmount = 20f;

    public void Collect(PlayerPickupFacade facade)
    {
        if (facade == null)
            return;

        facade.HealFlat(healAmount);
    }
}
