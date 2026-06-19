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

    [Header("Option Buttons")]
    [SerializeField] private Button option1Button;
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
    }

    private static void SetButtonState(Button button, bool interactable)
    {
        if (button == null)
            return;

        button.interactable = interactable;
    }
}
