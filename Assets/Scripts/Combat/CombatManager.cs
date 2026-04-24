using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CombatUIManager combatUI;
    [SerializeField] private BGMManager bgmManager;

    [Header("Player Data")]
    [SerializeField] private CombatStatsSO playerStats;
    [SerializeField] private SkillDataSO defaultPlayerSkill;

    [Header("Enemy Data")]
    [SerializeField] private CombatStatsSO defaultEnemyStats;

    private readonly ActionValueQueue actionQueue = new ActionValueQueue();
    private readonly EnemyCombatBrain enemyBrain = new EnemyCombatBrain();
    private readonly AttackAction playerAttack = new AttackAction(0, AttackType.Neutral);
    private readonly DefendAction playerDefend = new DefendAction();
    private readonly RunAction playerRun = new RunAction();

    private CombatEntity playerEntity;
    private CombatEntity enemyEntity;
    private CombatEntity currentActor;

    private ICombatAction queuedPlayerAction;
    private bool waitingForPlayerInput;
    private bool combatActive;

    private PlayerHealth playerHealth;
    private int skillCooldownRemaining;

    private void OnEnable()
    {
        EnemyAI.CombatRequested += HandleCombatRequested;
    }

    private void OnDisable()
    {
        EnemyAI.CombatRequested -= HandleCombatRequested;
    }

    private void Start()
    {
        EnsurePrototypeData();

        if (combatUI != null)
        {
            combatUI.OnAttackPressed += OnAttackSelected;
            combatUI.OnSkillPressed += OnSkillSelected;
            combatUI.OnDefendPressed += OnDefendSelected;
            combatUI.OnRunPressed += OnRunSelected;
        }
    }

    private void OnDestroy()
    {
        if (combatUI != null)
        {
            combatUI.OnAttackPressed -= OnAttackSelected;
            combatUI.OnSkillPressed -= OnSkillSelected;
            combatUI.OnDefendPressed -= OnDefendSelected;
            combatUI.OnRunPressed -= OnRunSelected;
        }
    }

    private void HandleCombatRequested(EnemyAI enemy)
    {
        if (combatActive)
            return;

        StartCombat(enemy);
    }

    private void StartCombat(EnemyAI enemy)
    {
        combatActive = true;
        skillCooldownRemaining = 0;
        playerHealth = PlayerController.Instance != null ? PlayerController.Instance.GetComponent<PlayerHealth>() : null;

        SetExplorationPaused(true);

        int maxHp = 1;
        int curHp = 1;
        if (playerHealth != null)
        {
            maxHp = Mathf.Max(1, Mathf.RoundToInt(playerHealth.MaxHealth));
            curHp = Mathf.Clamp(Mathf.RoundToInt(playerHealth.CurrentHealth), 0, maxHp);
        }
        else if (playerStats != null)
        {
            maxHp = Mathf.Max(1, playerStats.maxHP);
            curHp = maxHp;
        }

        playerEntity = new CombatEntity(
            owner: PlayerController.Instance != null ? PlayerController.Instance.GetComponent<MonoBehaviour>() : null,
            statsData: playerStats,
            isPlayer: true,
            displayName: "Player",
            currentHpOverride: curHp,
            maxHpOverride: maxHp);

        CombatStatsSO enemyStats = enemy != null && enemy.CombatStats != null ? enemy.CombatStats : defaultEnemyStats;
        enemyEntity = new CombatEntity(
            owner: enemy,
            statsData: enemyStats,
            isPlayer: false,
            displayName: "Enemy");

        actionQueue.Initialize(new List<CombatEntity> { playerEntity, enemyEntity });

        if (combatUI != null)
        {
            combatUI.SetVisible(true);
            combatUI.BindEntities(playerEntity, enemyEntity);
            combatUI.Refresh(playerEntity, enemyEntity);
            combatUI.AppendLog("Combat started.");
        }

        SyncPlayerHealthFromCombatEntity();
        StartCoroutine(CombatLoop());
    }

    private IEnumerator CombatLoop()
    {
        while (combatActive)
        {
            if (playerEntity == null || enemyEntity == null || playerEntity.IsDead || enemyEntity.IsDead)
            {
                EndCombat();
                yield break;
            }

            currentActor = actionQueue.AdvanceToNextActor();
            if (currentActor == null)
            {
                EndCombat();
                yield break;
            }

            if (currentActor.IsPlayer)
            {
                waitingForPlayerInput = true;
                queuedPlayerAction = null;
                if (combatUI != null)
                {
                    bool skillReady = skillCooldownRemaining <= 0 &&
                                      SkillAction.CanAfford(playerEntity, defaultPlayerSkill);
                    combatUI.SetPlayerTurnActions(true, skillReady, true, true);
                }

                while (waitingForPlayerInput)
                    yield return null;

                ICombatAction actionUsed = queuedPlayerAction;
                CombatResult playerResult = default;
                if (actionUsed != null)
                {
                    playerResult = actionUsed.Execute(playerEntity, enemyEntity, CurrentFloorType());
                    LogPlayerActionResult(playerResult);

                    SyncPlayerHealthFromCombatEntity();

                    if (playerResult.RunSucceeded)
                    {
                        EndCombat();
                        yield break;
                    }
                }

                if (actionUsed is SkillAction && defaultPlayerSkill != null && !playerResult.WasSkipped && !playerResult.RunSucceeded)
                    skillCooldownRemaining = defaultPlayerSkill.cooldownTurns;
                else if (skillCooldownRemaining > 0)
                    skillCooldownRemaining--;

                if (combatUI != null)
                    combatUI.SetInputEnabled(false);
            }
            else
            {
                if (combatUI != null)
                    combatUI.SetInputEnabled(false);

                CombatResult enemyResult = enemyBrain.TakeTurn(enemyEntity, playerEntity, CurrentFloorType());
                LogEnemyActionResult(enemyResult);
                SyncPlayerHealthFromCombatEntity();
                yield return new WaitForSeconds(0.2f);
            }

            actionQueue.CompleteTurn(currentActor);

            if (combatUI != null)
                combatUI.Refresh(playerEntity, enemyEntity);
        }
    }

    private void LogPlayerActionResult(CombatResult result)
    {
        if (combatUI == null)
            return;

        if (result.WasSkipped)
        {
            combatUI.AppendLog($"You: {result.ActionLabel} — not enough resources.");
            return;
        }

        if (result.ActionLabel == "Run")
        {
            combatUI.AppendLog(result.RunSucceeded ? "You: Run — succeeded." : "You: Run — failed.");
            return;
        }

        if (result.DefendApplied)
        {
            combatUI.AppendLog("You: Defend — you brace for the next hit.");
            return;
        }

        if (!string.IsNullOrEmpty(result.ActionLabel))
        {
            string mult = result.TypeMultiplier > 0f ? $" (type mult x{result.TypeMultiplier:0.##})" : string.Empty;
            combatUI.AppendLog($"You: {result.ActionLabel} — {result.DamageDealt} damage to enemy{mult}.");
        }
    }

    private void LogEnemyActionResult(CombatResult result)
    {
        if (combatUI == null)
            return;

        string mult = result.TypeMultiplier > 0f ? $" (type mult x{result.TypeMultiplier:0.##})" : string.Empty;
        combatUI.AppendLog($"Enemy: attack — {result.DamageDealt} damage to you{mult}.");
    }

    private BGMType CurrentFloorType()
    {
        return bgmManager != null ? bgmManager.CurrentFloorType : BGMType.None;
    }

    private void EndCombat()
    {
        SyncPlayerHealthFromCombatEntity();

        if (enemyEntity != null && enemyEntity.IsDead && enemyEntity.Owner is EnemyAI defeatedEnemy)
            defeatedEnemy.DefeatFromCombat();

        combatActive = false;
        StopAllCoroutines();
        SetExplorationPaused(false);
        playerHealth = null;

        if (combatUI != null)
        {
            combatUI.SetInputEnabled(false);
            combatUI.SetVisible(false);
        }
    }

    private void SyncPlayerHealthFromCombatEntity()
    {
        if (playerHealth == null || playerEntity == null)
            return;

        playerHealth.SetHealthFromCombat(playerEntity.CurrentHP);
    }

    private void SetExplorationPaused(bool paused)
    {
        if (PlayerController.Instance != null)
        {
            PlayerController controller = PlayerController.Instance.GetComponent<PlayerController>();
            if (controller != null)
                controller.enabled = !paused;

            if (paused)
            {
                Rigidbody2D playerRb = PlayerController.Instance.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                    playerRb.linearVelocity = Vector2.zero;
            }
        }

        EnemySpawner[] spawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);
        for (int i = 0; i < spawners.Length; i++)
        {
            if (spawners[i] != null)
                spawners[i].enabled = !paused;
        }

        EnemyAI[] allEnemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        for (int i = 0; i < allEnemies.Length; i++)
            allEnemies[i].SetMovementEnabled(!paused);
    }

    private void OnAttackSelected()
    {
        QueuePlayerAction(playerAttack);
    }

    private void OnSkillSelected()
    {
        if (!combatActive || !waitingForPlayerInput || playerEntity == null)
            return;

        if (skillCooldownRemaining > 0)
            return;

        if (!SkillAction.CanAfford(playerEntity, defaultPlayerSkill))
            return;

        QueuePlayerAction(new SkillAction(defaultPlayerSkill));
    }

    private void OnDefendSelected()
    {
        QueuePlayerAction(playerDefend);
    }

    private void OnRunSelected()
    {
        QueuePlayerAction(playerRun);
    }

    private void QueuePlayerAction(ICombatAction action)
    {
        if (!combatActive || !waitingForPlayerInput)
            return;

        queuedPlayerAction = action;
        waitingForPlayerInput = false;

        if (combatUI != null)
            combatUI.SetInputEnabled(false);
    }

    public void ConfigureForRuntime(CombatUIManager ui, BGMManager manager)
    {
        combatUI = ui;
        bgmManager = manager;
    }

    private void EnsurePrototypeData()
    {
        if (playerStats == null)
        {
            playerStats = ScriptableObject.CreateInstance<CombatStatsSO>();
            playerStats.maxHP = 120;
            playerStats.maxMP = 40;
            playerStats.attack = 14;
            playerStats.defense = 8;
            playerStats.speed = 110;
        }

        if (defaultEnemyStats == null)
        {
            defaultEnemyStats = ScriptableObject.CreateInstance<CombatStatsSO>();
            defaultEnemyStats.maxHP = 90;
            defaultEnemyStats.maxMP = 20;
            defaultEnemyStats.attack = 11;
            defaultEnemyStats.defense = 6;
            defaultEnemyStats.speed = 95;
        }

        if (defaultPlayerSkill == null)
        {
            defaultPlayerSkill = ScriptableObject.CreateInstance<SkillDataSO>();
            defaultPlayerSkill.skillName = "Axe Swing";
            defaultPlayerSkill.attackType = AttackType.Jazz;
            defaultPlayerSkill.basePower = 16;
            defaultPlayerSkill.costType = SkillCostType.MP;
            defaultPlayerSkill.costValue = 6;
            defaultPlayerSkill.cooldownTurns = 2;
        }
    }
}
