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

    private void Awake()
    {
        if (levelText != null && levelText.font == null && TMP_Settings.defaultFontAsset != null)
            levelText.font = TMP_Settings.defaultFontAsset;
    }

    void Start()
    {
        UpdateExp(0f, 100f);

        if (expRef != null)
            Bind();
    }

    private void Bind()
    {
        expRef.OnExpChanged += UpdateExp;
        expRef.OnLevelUp += UpdateLevel;
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
