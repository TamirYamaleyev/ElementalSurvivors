using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExpbarUIController : MonoBehaviour
{
    [SerializeField] private PlayerEXP expRef;
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private string levelPrefix = "LV: ";

    void Start()
    {
        UpdateExp(0f, 100f);

        if (expRef != null)
            Bind(expRef);
    }

    private void Bind(PlayerEXP exp)
    {
        exp.OnExpChanged += UpdateExp;
        exp.OnLevelUp += UpdateLevel;
    }

    private void UpdateExp(float current, float max)
    {
        fillImage.fillAmount = current / max;
    }

    private void UpdateLevel(int level)
    {
        levelText.text = $"{levelPrefix}{level}";
    }
}
