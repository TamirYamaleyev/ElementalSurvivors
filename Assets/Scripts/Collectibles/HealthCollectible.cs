using UnityEngine;

public class HealthCollectible : MonoBehaviour, ICollectible
{
    [SerializeField] AudioClip sfx;

    [SerializeField] private float healAmount = 20f;

    public void Collect(PlayerPickupFacade facade)
    {
        if (facade == null)
            return;

        AudioManager.Instance.PlaySfx(sfx);

        facade.HealFlat(healAmount);
    }
}
