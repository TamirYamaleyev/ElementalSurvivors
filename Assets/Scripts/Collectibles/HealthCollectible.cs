using UnityEngine;

public class HealthCollectible : MonoBehaviour, ICollectible
{
    [SerializeField] AudioClip sfx;
    [SerializeField, Range(0f, 1f)] private float sfxVolume;

    [SerializeField] private float healAmount = 20f;

    public void Collect(PlayerPickupFacade facade)
    {
        if (facade == null)
            return;

        AudioManager.Instance.PlaySfx(sfx, sfxVolume);

        facade.HealFlat(healAmount);
    }
}
