using UnityEngine;

public class Health : MonoBehaviour
{
    public float hp = 100;

    public void TakeDamage(float amount)
    {
        hp -= amount;

        Debug.Log($"Took {amount} damage\nHP: {hp}");
    }
}
