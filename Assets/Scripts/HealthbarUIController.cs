using UnityEngine;
using UnityEngine.UI;

public class HealthbarUIController : MonoBehaviour
{
    [SerializeField] private PlayerHealth healthRef;
    [SerializeField] private Image fillImage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateHealth(100, 100);

        if (healthRef != null)
            Bind(healthRef);
    }

    public void Bind(PlayerHealth health)
    {
        health.OnHealthChanged += UpdateHealth;
    }

    public void UpdateHealth(float current, float max)
    {
        fillImage.fillAmount = current / max;
    }
}
