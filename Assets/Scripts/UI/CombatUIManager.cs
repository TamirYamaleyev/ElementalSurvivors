using System;
using UnityEngine;
using UnityEngine.UI;

public class CombatUIManager : MonoBehaviour
{
    [Header("Panel Root")]
    [SerializeField] private GameObject combatRoot;

    [Header("HUD")]
    [SerializeField] private CombatEntityHUD playerHUD;
    [SerializeField] private CombatEntityHUD focusedEnemyHUD;

    [Header("Log")]
    [SerializeField] private CombatLogView combatLog;

    [Header("Buttons")]
    [SerializeField] private Button attackButton;
    [SerializeField] private Button skillButton;
    [SerializeField] private Button defendButton;
    [SerializeField] private Button runButton;

    public event Action OnAttackPressed;
    public event Action OnSkillPressed;
    public event Action OnDefendPressed;
    public event Action OnRunPressed;

    private void Awake()
    {
        RebindButtonListeners();
        SetVisible(false);
    }

    public void SetVisible(bool visible)
    {
        if (combatRoot != null)
            combatRoot.SetActive(visible);
    }

    public void BindEntities(CombatEntity player, CombatEntity enemy)
    {
        if (combatLog != null)
            combatLog.Clear();

        if (playerHUD != null) playerHUD.Bind(player);
        if (focusedEnemyHUD != null) focusedEnemyHUD.Bind(enemy);
    }

    public void Refresh(CombatEntity player, CombatEntity enemy)
    {
        if (playerHUD != null) playerHUD.Refresh(player);
        if (focusedEnemyHUD != null) focusedEnemyHUD.Refresh(enemy);
    }

    public void AppendLog(string line)
    {
        if (combatLog != null)
            combatLog.Append(line);
    }

    /// <summary>All four buttons same state (e.g. enemy turn — all off).</summary>
    public void SetInputEnabled(bool enabled)
    {
        SetPlayerTurnActions(enabled, enabled, enabled, enabled);
    }

    public void SetPlayerTurnActions(bool attack, bool skill, bool defend, bool run)
    {
        if (attackButton != null) attackButton.interactable = attack;
        if (skillButton != null) skillButton.interactable = skill;
        if (defendButton != null) defendButton.interactable = defend;
        if (runButton != null) runButton.interactable = run;
    }

    public void ConfigureRuntime(
        GameObject root,
        CombatEntityHUD playerEntityHUD,
        CombatEntityHUD enemyEntityHUD,
        Button attack,
        Button skill,
        Button defend,
        Button run,
        CombatLogView logView = null)
    {
        combatRoot = root;
        playerHUD = playerEntityHUD;
        focusedEnemyHUD = enemyEntityHUD;
        attackButton = attack;
        skillButton = skill;
        defendButton = defend;
        runButton = run;
        combatLog = logView;
        RebindButtonListeners();
        SetVisible(false);
    }

    private void RebindButtonListeners()
    {
        if (attackButton != null)
        {
            attackButton.onClick.RemoveAllListeners();
            attackButton.onClick.AddListener(() => OnAttackPressed?.Invoke());
        }

        if (skillButton != null)
        {
            skillButton.onClick.RemoveAllListeners();
            skillButton.onClick.AddListener(() => OnSkillPressed?.Invoke());
        }

        if (defendButton != null)
        {
            defendButton.onClick.RemoveAllListeners();
            defendButton.onClick.AddListener(() => OnDefendPressed?.Invoke());
        }

        if (runButton != null)
        {
            runButton.onClick.RemoveAllListeners();
            runButton.onClick.AddListener(() => OnRunPressed?.Invoke());
        }
    }
}
