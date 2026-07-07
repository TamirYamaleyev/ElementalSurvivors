using UnityEngine;

public class EXPOrb : MonoBehaviour, ICollectible
{
    [SerializeField] AudioClip sfx;

    public float expToGive;

    public void Collect(PlayerPickupFacade facade)
    {
        if (facade == null)
            return;

        AudioManager.Instance.PlaySfx(sfx);

        facade.AddExp(expToGive);
    }
}
