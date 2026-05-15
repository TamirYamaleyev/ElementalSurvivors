using UnityEngine;

public class EXPOrb : MonoBehaviour
{
    public float expToGive;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerEXP>(out var playerEXP))
        {
            playerEXP.AddExp(expToGive);
        }
    }
}
