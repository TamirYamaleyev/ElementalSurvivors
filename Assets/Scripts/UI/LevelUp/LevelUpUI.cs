using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpUI : MonoBehaviour
{
    [SerializeField] private GameObject levelUpPanel;

    private UpgradeOption[] currentOptions;

    [Header("Option Buttons")]
    [SerializeField] private Button option1Button;
    [SerializeField] private Image option1Icon;
    [SerializeField] private TextMeshProUGUI option1Name;
    [SerializeField] private TextMeshProUGUI option1Level;
    [SerializeField] private TextMeshProUGUI option1ElementName;
    [SerializeField] private TextMeshProUGUI option1Description;

    [SerializeField] private Button option2Button;
    [SerializeField] private Image option2Icon;
    [SerializeField] private TextMeshProUGUI option2Name;
    [SerializeField] private TextMeshProUGUI option2Level;
    [SerializeField] private TextMeshProUGUI option2ElementName;
    [SerializeField] private TextMeshProUGUI option2Description;

    [SerializeField] private Button option3Button;
    [SerializeField] private Image option3Icon;
    [SerializeField] private TextMeshProUGUI option3Name;
    [SerializeField] private TextMeshProUGUI option3Level;
    [SerializeField] private TextMeshProUGUI option3ElementName;
    [SerializeField] private TextMeshProUGUI option3Description;

    private void OnEnable()
    {
        option1Button.onClick.AddListener(() => SelectOption(0));
        option2Button.onClick.AddListener(() => SelectOption(1));
        option3Button.onClick.AddListener(() => SelectOption(2));
    }

    private void OnDisable()
    {
        option1Button.onClick.RemoveAllListeners();
        option2Button.onClick.RemoveAllListeners();
        option3Button.onClick.RemoveAllListeners();
    }
    public void ShowChoices(List<UpgradeOption> options)
    {
        currentOptions = options.ToArray();

        UpdateButtons();

        levelUpPanel.SetActive(true);
    }

    public void ChoiceSelected()
    {
        Time.timeScale = 1f;

        AudioManager.Instance.UnPauseBGM();
        AudioManager.Instance.UnPauseSFX();

        levelUpPanel.SetActive(false);
    }

    private void UpdateButtons()
    {
        SetupButton(0, option1Icon, option1Name, option1Level, option1ElementName, option1Description);
        SetupButton(1, option2Icon, option2Name, option2Level, option2ElementName, option2Description);
        SetupButton(2, option3Icon, option3Name, option3Level, option3ElementName, option3Description);
    }

    private void SetupButton(
        int index, 
        Image icon,
        TMP_Text name,
        TMP_Text level,
        TMP_Text element,
        TMP_Text description)
    {
        if (currentOptions == null || index >= currentOptions.Length)
            return;

        var option = currentOptions[index];

        icon.sprite = option.Icon;
        name.text = option.Name;
        level.text = option.LevelText;

        element.text = option.Element.name;
        element.color = option.Element.color;
        description.text = option.Description;

        //WeaponDefinition def;

        //if (option.IsUnlock)
        //{
        //    def = option.unlockDefinition;

        //    icon.sprite = def.icon;
        //    name.text = def.weaponName;
        //    level.text = "Unlock";
        //}
        //else
        //{
        //    var weapon = option.weapon;

        //    def = weapon.definition;

        //    icon.sprite = def.icon;
        //    name.text = def.weaponName;

        //    level.text = $"Level {weapon.level} -> {weapon.level + 1}";
        //}

        //element.text = def.element.name;
        //element.color = def.element.color;

        //description.text = def.description;
    }

    private void SelectOption(int index)
    {
        if (currentOptions == null || index >= currentOptions.Length)
            return;

        currentOptions[index].Apply();

        //var option = currentOptions[index];

        //if (option.IsUnlock)
        //    weaponSystem.UnlockWeapon(option.unlockDefinition);
        //else
        //    option.weapon.LevelUp();

        ChoiceSelected();
    }
}
