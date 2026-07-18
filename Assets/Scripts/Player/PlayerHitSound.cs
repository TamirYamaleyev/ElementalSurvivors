using UnityEngine;

public class PlayerHitSound : MonoBehaviour
{
    [SerializeField] private AudioClip hitSfx;
    [SerializeField] private float cooldown = 0.2f;

    private float cooldownTimer;

    void Update()
    {
        if (cooldownTimer > 0)
            cooldownTimer -= Time.deltaTime;
    }

    public void PlayHitSound()
    {
        if (cooldownTimer > 0)
            return;

        AudioManager.Instance.PlaySfx(hitSfx);
        cooldownTimer = cooldown;
    }
}
