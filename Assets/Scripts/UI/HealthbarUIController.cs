using UnityEngine;
using UnityEngine.UI;

public class HealthbarUIController : MonoBehaviour
{
    [SerializeField] private PlayerHealth healthRef;
    [SerializeField] private Image fillImage;
    //[SerializeField] private Image fillRoses;

    [SerializeField] private float lerpSpeed = 5f;

    private float targetFillAmount;
    private float currentFillAmount;

    void Start()
    {
        UpdateHealth(100, 100);

        if (healthRef != null)
            Bind(healthRef);
    }

    private void Update()
    {
        currentFillAmount = Mathf.Lerp(
            currentFillAmount,
            targetFillAmount,
            Time.deltaTime * lerpSpeed
        );

        fillImage.fillAmount = currentFillAmount;
        //fillRoses.fillAmount = currentFillAmount;
    }

    public void Bind(PlayerHealth health)
    {
        health.OnHealthChanged += UpdateHealth;
    }

    public void UpdateHealth(float current, float max)
    {
        targetFillAmount = current / max;
    }
}