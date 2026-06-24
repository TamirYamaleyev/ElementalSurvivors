using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns one enemy per reaction pair and plays reaction burst VFX from
/// <see cref="ReactionVfxCatalogSO"/> without applying base elemental status visuals.
/// </summary>
public sealed class ReactionVfxShowcaseBootstrap : MonoBehaviour
{
    public static Transform VfxContainer { get; private set; }

    [SerializeField] private Enemy enemyPrefab = null!;
    [SerializeField] private int columns = 5;
    [SerializeField] private float columnSpacing = 2.2f;
    [SerializeField] private float rowSpacing = 2.6f;
    [SerializeField] private bool loopReactionVfx = true;

    private static readonly (StatusType A, StatusType B)[] Pairs =
    {
        (StatusType.Fire, StatusType.Water),
        (StatusType.Fire, StatusType.Earth),
        (StatusType.Fire, StatusType.Wind),
        (StatusType.Fire, StatusType.Lightning),
        (StatusType.Water, StatusType.Wind),
        (StatusType.Water, StatusType.Earth),
        (StatusType.Water, StatusType.Lightning),
        (StatusType.Wind, StatusType.Earth),
        (StatusType.Wind, StatusType.Lightning),
        (StatusType.Earth, StatusType.Lightning),
    };

    private static readonly string[] ReactionNames =
    {
        "Vaporize",
        "Crystallize",
        "Scorching Wind",
        "Explosion",
        "Hail",
        "Growth",
        "Electrowetting",
        "Dust Sand Storm",
        "Magnetism",
        "Static Charge",
    };

    // Matches ReactionBurstParticleBootstrap destroy-after values per pair above.
    private static readonly float[] ReactionLoopSeconds =
    {
        4f, 2.4f, 0.5f, 0.85f, 2.4f, 2.4f, 0.5f, 2.6f, 1.2f, 1.8f,
    };

    private void Awake()
    {
        VfxContainer = transform;
    }

    private void OnDestroy()
    {
        if (VfxContainer == transform)
            VfxContainer = null;
    }

    private void Start()
    {
        var status = FindAnyObjectByType<StatusSystem>();
        var registry = FindAnyObjectByType<EnemyRegistry>();
        if (enemyPrefab == null || status == null)
        {
            Debug.LogWarning("[ReactionVfxShowcase] Assign enemy prefab; scene needs StatusSystem.");
            return;
        }

        if (registry == null)
        {
            Debug.LogWarning("[ReactionVfxShowcase] EnemyRegistry not found in scene.", this);
            return;
        }

        DisableSceneInterruptions();
        TmpFontUtility.EnsureAllInScene();
        HideGameplayUi();

        var origin = transform.position;
        var n = Pairs.Length;
        var gridColumns = Mathf.Max(1, columns);
        var rows = Mathf.CeilToInt(n / (float)gridColumns);

        for (var i = 0; i < n; i++)
        {
            var col = i % gridColumns;
            var row = i / gridColumns;
            var x = (col - (gridColumns - 1) * 0.5f) * columnSpacing;
            var y = ((rows - 1) * 0.5f - row) * rowSpacing;
            var pos = origin + new Vector3(x, y, 0f);

            var enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
            enemy.ConfigureSystems(status, registry);
            enemy.Initialize(status, registry);

            PrepareShowcaseEnemy(enemy, registry);

            var (a, b) = Pairs[i];
            var reactionName = i < ReactionNames.Length ? ReactionNames[i] : $"{a} + {b}";
            ReactionShowcaseLabel.Create(enemy.transform, reactionName, a, b);

            status.SpawnReactionVfx(enemy, a, b);

            if (loopReactionVfx)
            {
                var interval = i < ReactionLoopSeconds.Length ? ReactionLoopSeconds[i] : 2.4f;
                StartCoroutine(LoopReactionVfx(status, enemy, a, b, interval));
            }
        }
    }

    private static void HideGameplayUi()
    {
        var hud = GameObject.Find("HUD");
        if (hud != null)
            hud.SetActive(false);

        var levelUpRoot = GameObject.Find("LevelUpUIRoot");
        if (levelUpRoot != null)
            levelUpRoot.SetActive(false);
    }

    private static void DisableSceneInterruptions()
    {
        var spawner = FindAnyObjectByType<EnemySpawner>();
        if (spawner != null)
            spawner.enabled = false;

        var weaponSystem = FindAnyObjectByType<WeaponSystem>();
        if (weaponSystem != null)
            weaponSystem.enabled = false;
    }

    private static void PrepareShowcaseEnemy(Enemy enemy, EnemyRegistry registry)
    {
        var ai = enemy.GetComponent<EnemyAI>();
        if (ai != null)
            ai.SetGameplayEnabled(false);

        var statusPresenter = enemy.GetComponent<ElementalStatusVfxPresenter>();
        if (statusPresenter != null)
            statusPresenter.enabled = false;

        registry.Register(enemy);
    }

    private static IEnumerator LoopReactionVfx(
        StatusSystem status,
        Enemy enemy,
        StatusType a,
        StatusType b,
        float interval)
    {
        while (enemy != null)
        {
            yield return new WaitForSeconds(interval);
            if (enemy != null)
                status.SpawnReactionVfx(enemy, a, b);
        }
    }
}
