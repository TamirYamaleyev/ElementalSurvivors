using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class ExpbarUIController : MonoBehaviour
{
    [SerializeField] private PlayerEXP expRef;
    [SerializeField] private Image fillImage;

    void Start()
    {
        UpdateExp(0f, 100f);

        if (expRef != null)
            Bind(expRef);
    }

    private void Bind(PlayerEXP exp)
    {
        exp.OnExpChanged += UpdateExp;
    }

    private void UpdateExp(float current, float max)
    {
        fillImage.fillAmount = current / max;
    }
}
