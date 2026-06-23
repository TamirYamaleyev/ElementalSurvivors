using UnityEngine;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    [SerializeField] private string GAME_SCENE_NAME;

    public float hp = 100;

    public void TakeDamage(float amount)
    {
        hp -= amount;

        Debug.Log($"Took {amount} damage\nHP: {hp}");

        if (hp <= 0 )
        {
            SceneManager.LoadScene( GAME_SCENE_NAME );
        }
    }
}
