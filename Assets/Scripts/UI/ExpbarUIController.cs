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

    [SerializeField] private float lerpSpeed = 5f;

    private float targetFillAmount;
    private float currentFillAmount;

    private Vector2 targetMaskAnchor;

    void Start()
    {
        UpdateExp(0f, 100f);

        if (expRef != null)
            Bind();
    }

    private void Update()
    {
        currentFillAmount = Mathf.Lerp(
            currentFillAmount,
            targetFillAmount,
            Time.deltaTime * lerpSpeed
        );

        fillImage.fillAmount = currentFillAmount;

        textMask.anchorMax = Vector2.Lerp(
            textMask.anchorMax,
            targetMaskAnchor,
            Time.deltaTime * lerpSpeed
        );
    }

    private void Bind()
    {
        expRef.OnExpChanged += UpdateExp;
        expRef.OnLevelUp += UpdateLevel;
    }

    private void UpdateExp(float current, float max)
    {
        targetFillAmount = current / max;
        targetMaskAnchor = new Vector2(targetFillAmount, 1f);
    }

    private void UpdateLevel(int level)
    {
        string text = $"{levelPrefix}{level}";

        levelText.text = text;
        levelTextDark.text = text;
    }
}