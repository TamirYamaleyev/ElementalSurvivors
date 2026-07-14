//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;

//public class LevelUpUI : MonoBehaviour
//{
//    [SerializeField] private GameObject levelUpPanel;
//    [SerializeField] private PlayerEXP expRef;
//    [SerializeField] private WeaponSystem weaponSystem;

//    [Header("Level-Up Options")]
//    [SerializeField] private WeaponDefinition[] levelUpOptions;
//    [SerializeField] private Button[] optionButtons;

//    private bool buttonsBound;

//    void Start()
//    {
//        if (weaponSystem == null)
//            weaponSystem = FindFirstObjectByType<WeaponSystem>();

//        BindOptionButtons();

//        if (expRef != null)
//            expRef.OnLevelUp += HandleLevelUp;
//    }

//    void OnDestroy()
//    {
//        if (expRef != null)
//            expRef.OnLevelUp -= HandleLevelUp;
//    }

//    private void BindOptionButtons()
//    {
//        if (buttonsBound || optionButtons == null)
//            return;

//        for (int i = 0; i < optionButtons.Length; i++)
//        {
//            var button = optionButtons[i];
//            if (button == null)
//                continue;

//            int index = i;
//            button.onClick.RemoveAllListeners();
//            button.onClick.AddListener(() => Choose(index));
//        }

//        buttonsBound = true;
//        RefreshOptionLabels();
//    }

//    private void HandleLevelUp(int level)
//    {
//        if (!GamePauseController.CanOpenPauseMenu)
//            return;

//        BindOptionButtons();
//        RefreshOptionButtons();
//        GamePauseController.RequestPause(GamePauseController.PauseReason.LevelUp);
//        if (levelUpPanel != null)
//            levelUpPanel.SetActive(true);
//    }

//    public void Choose(int index)
//    {
//        if (weaponSystem == null || levelUpOptions == null)
//        {
//            ChoiceSelected();
//            return;
//        }

//        if (index < 0 || index >= levelUpOptions.Length)
//        {
//            ChoiceSelected();
//            return;
//        }

//        var def = levelUpOptions[index];
//        if (def != null)
//            weaponSystem.TryLevelUp(def);

//        ChoiceSelected();
//    }

//    public void ChoiceSelected()
//    {
//        GamePauseController.ReleasePause(GamePauseController.PauseReason.LevelUp);
//        if (levelUpPanel != null)
//            levelUpPanel.SetActive(false);
//    }

//    public void HideLevelUpPanel()
//    {
//        if (levelUpPanel != null)
//            levelUpPanel.SetActive(false);
//    }

//    private void RefreshOptionButtons()
//    {
//        if (optionButtons == null)
//            return;

//        for (int i = 0; i < optionButtons.Length; i++)
//        {
//            var button = optionButtons[i];
//            if (button == null)
//                continue;

//            WeaponDefinition def = null;
//            if (levelUpOptions != null && i < levelUpOptions.Length)
//                def = levelUpOptions[i];

//            bool canPick = def != null && weaponSystem != null && !weaponSystem.IsMaxed(def);
//            button.interactable = canPick;
//        }

//        RefreshOptionLabels();
//    }

//    private void RefreshOptionLabels()
//    {
//        if (optionButtons == null || levelUpOptions == null)
//            return;

//        for (int i = 0; i < optionButtons.Length && i < levelUpOptions.Length; i++)
//        {
//            var button = optionButtons[i];
//            var def = levelUpOptions[i];
//            if (button == null || def == null)
//                continue;

//            var label = button.GetComponentInChildren<TMP_Text>(true);
//            if (label == null)
//                continue;

//            string status = def.appliedStatus == StatusType.None
//                ? "No Status"
//                : $"{def.appliedStatus} Status";
//            label.text = $"{def.weaponName}\n{status}\nMax {def.MaxLevel} Levels";
//        }
//    }
//}
