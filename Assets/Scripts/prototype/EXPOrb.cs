using UnityEngine;

public class EXPOrb : MonoBehaviour, ICollectible
{
    public float expToGive;

    public void Collect(PlayerPickupFacade facade)
    {
        if (facade == null)
            return;

        facade.AddExp(expToGive);
    }
}
