using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExpbarUIController : MonoBehaviour
{
    [SerializeField] private PlayerEXP expRef;

    [SerializeField] private Image fillImage;

    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text levelTextDark;
    [SerializeField] private RectTransform textMask;

    [SerializeField] private string levelPrefix = "LV: ";

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
        float fillAmount = current / max;

        fillImage.fillAmount = fillAmount;

        textMask.anchorMax = new Vector2(fillAmount, 1f);
    }

    private void UpdateLevel(int level)
    {
        string text = $"{levelPrefix}{level}";

        levelText.text = text;
        levelTextDark.text = text;
    }
}
