using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpUI : MonoBehaviour
{
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private PlayerEXP expRef;

    [Header("Upgrade Targets")]
    [SerializeField] private PlayerDefaultWeapon spearWeapon;
    [SerializeField] private OrbitWeapon orbitWeapon;
    [SerializeField] private BoomerangController boomerangWeapon;

    [SerializeField] private LevelUpOption[] data;

    [Header("Option Buttons")]
    [SerializeField] private Button option1Button;
    [SerializeField] private Image option1Icon;
    [SerializeField] private TextMeshProUGUI option1Name;
    [SerializeField] private TextMeshProUGUI option1Level;
    [SerializeField] private TextMeshProUGUI option1ElementName;
    [SerializeField] private TextMeshProUGUI option1Description;

    [SerializeField] private Button option2Button;
    [SerializeField] private Button option3Button;

    void Start()
    {
        if (expRef != null)
            Bind();
    }

    private void Bind()
    {
        expRef.OnLevelUp += HandleLevelUp;
    }

    private void HandleLevelUp(int level)
    {
        RefreshOptionButtons();
        Time.timeScale = 0f;
        levelUpPanel.SetActive(true);
    }

    public void ChoiceSelected()
    {
        Time.timeScale = 1f;
        levelUpPanel.SetActive(false);
    }

    private void RefreshOptionButtons()
    {
        SetButtonState(option1Button, spearWeapon == null || !spearWeapon.IsMaxed);
        SetButtonState(option2Button, orbitWeapon == null || !orbitWeapon.IsMaxed);
        SetButtonState(option3Button, boomerangWeapon == null || !boomerangWeapon.IsMaxed);

        UpdateButtons();
    }

    private static void SetButtonState(Button button, bool interactable)
    {
        if (button == null)
            return;

        button.interactable = interactable;
    }

    private void UpdateButtons()
    {
        //option1Button.onClick.AddListener();
        option1Icon.sprite = data[0].icon;
        option1Name.text = data[0].displayName;
        option1ElementName.text = data[0].element.name;
        option1ElementName.color = data[0].element.color;
        option1Description.text = data[0].description;
    }
}
