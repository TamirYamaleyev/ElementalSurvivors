#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;

/// <summary>
/// One-shot pipeline: placeholder sprite sheets, clips, animator controllers, Visual child setup.
/// Run: Unity -batchmode -quit -executeMethod CharacterAnimationPipeline.GenerateAll
/// Production art (sliced PNGs under Art/Characters): CharacterAnimationPipeline.BindProductionCharacterSprites
/// </summary>
public static class CharacterAnimationPipeline
{
    const int FrameW = 32;
    const int FrameH = 32;
    const int FrameCount = 8;
    /// <summary>Uniform scale on child Visual (high-res sprites use large PPU).</summary>
    const float VisualDisplayScale = 0.24f;
    const int EnemyCycleFrameCount = 9;
    const float EnemyIdleCycleDuration = 0.9f;
    const float EnemyWalkCycleDuration = 0.65f;

    /// <summary>
    /// Rebuilds player clips from dev SampleScene art (Assets/Sprites/IMG_0600.png).
    /// Unity -batchmode -quit -executeMethod CharacterAnimationPipeline.EnsureDevPlayerAnimationAssets
    /// </summary>
    public static void EnsureDevPlayerAnimationAssets()
    {
        try
        {
            EnsureFolders();
            const string devTexturePath = "Assets/Sprites/IMG_0600.png";
            var devSprites = LoadSpritesOrdered(devTexturePath);
            if (devSprites.Count == 0)
                throw new System.InvalidOperationException("No sprites in " + devTexturePath);

            var idleFrames = devSprites.Count >= 2
                ? new List<Sprite> { devSprites[0], devSprites[1] }
                : new List<Sprite> { devSprites[0] };
            var walkFrames = devSprites.Count >= 2
                ? new List<Sprite> { devSprites[0], devSprites[1] }
                : new List<Sprite> { devSprites[0] };
            var attackFrames = new List<Sprite> { devSprites[devSprites.Count - 1] };

            var playerIdle = BuildOrUpdateSpriteClipFromFrames(
                "Assets/Animation/Clips/player_idle.anim", idleFrames, 0.5f, loop: true);
            var playerWalk = BuildOrUpdateSpriteClipFromFrames(
                "Assets/Animation/Clips/player_walk.anim", walkFrames, 0.35f, loop: true);
            var playerAttack = BuildOrUpdateSpriteClipFromFrames(
                "Assets/Animation/Clips/player_attack.anim", attackFrames, 0.25f, loop: false);

            AssignClipsToExistingController(
                "Assets/Animation/Controllers/AC_Player.controller",
                playerIdle,
                playerWalk,
                playerAttack,
                deathClip: null);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CharacterAnimationPipeline] Dev player animation assets rebuilt from IMG_0600.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[CharacterAnimationPipeline] EnsureDevPlayerAnimationAssets: " + ex);
            if (Application.isBatchMode)
                EditorApplication.Exit(1);
        }
    }

    /// <summary>Unity -batchmode -quit -executeMethod CharacterAnimationPipeline.SetupPlayerAnimatorInSampleScene</summary>
    public static void SetupPlayerAnimatorInSampleScene()
    {
        try
        {
            SetupPlayerInSampleScene();
            AssetDatabase.SaveAssets();
            Debug.Log("[CharacterAnimationPipeline] Player animator visual wired in SampleScene.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[CharacterAnimationPipeline] SetupPlayerAnimatorInSampleScene: " + ex);
            if (Application.isBatchMode)
                EditorApplication.Exit(1);
        }
    }

    /// <summary>
    /// Regenerates placeholder sprite sheets and animation clips when production art is missing.
    /// Unity -batchmode -quit -executeMethod CharacterAnimationPipeline.EnsurePlaceholderAnimationAssets
    /// </summary>
    public static void EnsurePlaceholderAnimationAssets()
    {
        try
        {
            EnsureFolders();
            WritePlaceholderSheets();
            AssetDatabase.Refresh();

            var playerSprites = ImportAndSlice("Assets/Art/Characters/Player/player_sheet.png", "p");
            var enemySprites = ImportAndSlice("Assets/Art/Characters/Enemy/enemy_sheet.png", "e");

            var playerIdle = BuildSpriteClip("Assets/Animation/Clips/player_idle.anim", playerSprites, new[] { 0, 1 }, 0.5f, loop: true);
            var playerWalk = BuildSpriteClip("Assets/Animation/Clips/player_walk.anim", playerSprites, new[] { 2, 3, 4 }, 0.35f, loop: true);
            var playerAttack = BuildSpriteClip("Assets/Animation/Clips/player_attack.anim", playerSprites, new[] { 5, 6 }, 0.25f, loop: false);

            var enemyCycleIndices = EnemyCycleFrameIndices(enemySprites);
            var enemyIdle = BuildSpriteClip(
                "Assets/Animation/Clips/enemy_idle.anim", enemySprites, enemyCycleIndices, EnemyIdleCycleDuration, loop: true);
            var enemyWalk = BuildSpriteClip(
                "Assets/Animation/Clips/enemy_walk.anim", enemySprites, enemyCycleIndices, EnemyWalkCycleDuration, loop: true);
            var enemyAttack = BuildSpriteClip(
                "Assets/Animation/Clips/enemy_attack.anim",
                enemySprites,
                enemyCycleIndices.Length >= 3
                    ? new[] { enemyCycleIndices[^3], enemyCycleIndices[^2], enemyCycleIndices[^1] }
                    : enemyCycleIndices,
                0.25f,
                loop: false);
            var enemyDeath = BuildSpriteClip(
                "Assets/Animation/Clips/enemy_death.anim",
                enemySprites,
                new[] { enemyCycleIndices[^1] },
                0.4f,
                loop: false);

            BuildController(
                "Assets/Animation/Controllers/AC_Player.controller",
                playerIdle,
                playerWalk,
                playerAttack,
                includeDeath: false);

            BuildController(
                "Assets/Animation/Controllers/AC_Enemy.controller",
                enemyIdle,
                enemyWalk,
                enemyAttack,
                includeDeath: true,
                deathClip: enemyDeath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CharacterAnimationPipeline] Placeholder animation assets regenerated.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[CharacterAnimationPipeline] EnsurePlaceholderAnimationAssets: " + ex);
            if (Application.isBatchMode)
                EditorApplication.Exit(1);
        }
    }

    /// <summary>
    /// Builds player clips from Assets/Art/Characters/Player/player_walk.png and Player_attak.png,
    /// then wires AC_Player on SampleScene Player/Visual.
    /// Unity -batchmode -quit -executeMethod CharacterAnimationPipeline.BindArtFolderPlayerSprites
    /// </summary>
    public static void BindArtFolderPlayerSprites()
    {
        try
        {
            EnsureFolders();

            const string walkPath = "Assets/Art/Characters/Player/player_walk.png";
            const string attackPath = "Assets/Art/Characters/Player/Player_attak.png";

            if (!File.Exists(walkPath))
                throw new FileNotFoundException("Missing " + walkPath);
            if (!File.Exists(attackPath))
                throw new FileNotFoundException("Missing " + attackPath);

            EnsurePlayerWalkSpriteSheet(walkPath);

            var walkSprites = LoadSpritesOrdered(walkPath)
                .Where(s => s.rect.width >= 64 && s.rect.height >= 64)
                .ToList();
            var attackSprites = LoadSpritesOrdered(attackPath)
                .Where(s => s.rect.width >= 64 && s.rect.height >= 64)
                .ToList();

            if (walkSprites.Count == 0)
                throw new System.InvalidOperationException("No usable walk sprites in player_walk.png.");
            if (attackSprites.Count == 0)
                throw new System.InvalidOperationException("No usable attack sprites in Player_attak.png.");

            var idleFrames = walkSprites.Count >= 2
                ? walkSprites.GetRange(0, 2)
                : new List<Sprite> { walkSprites[0] };

            var playerIdle = BuildOrUpdateSpriteClipFromFrames(
                "Assets/Animation/Clips/player_idle.anim", idleFrames, 0.5f, loop: true);
            var playerWalk = BuildOrUpdateSpriteClipFromFrames(
                "Assets/Animation/Clips/player_walk.anim", walkSprites, 0.35f, loop: true);
            var playerAttack = BuildOrUpdateSpriteClipFromFrames(
                "Assets/Animation/Clips/player_attack.anim", attackSprites, 0.25f, loop: false);

            AssignClipsToExistingController(
                "Assets/Animation/Controllers/AC_Player.controller",
                playerIdle,
                playerWalk,
                playerAttack,
                deathClip: null);

            SetupPlayerInSampleScene();
            AssignDefaultPlayerVisualSprite(walkSprites[0]);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[CharacterAnimationPipeline] Bound art-folder player sprites: walk={walkSprites.Count}, attack={attackSprites.Count}.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[CharacterAnimationPipeline] BindArtFolderPlayerSprites: " + ex);
            if (Application.isBatchMode)
                EditorApplication.Exit(1);
        }
    }

    /// <summary>
    /// Re-slices enemy_sheet.png, rebuilds AC_Enemy clips, and updates Level1/2/3 enemy prefabs.
    /// Unity -batchmode -quit -executeMethod CharacterAnimationPipeline.BindArtFolderEnemySprites
    /// </summary>
    public static void BindArtFolderEnemySprites()
    {
        try
        {
            EnsureFolders();

            const string sheetPath = "Assets/Art/Characters/Enemy/enemy_sheet.png";
            if (!File.Exists(sheetPath))
                throw new FileNotFoundException("Missing " + sheetPath);

            EnsureEnemySheetSpriteSheet(sheetPath);
            EnsureSpriteSheetPixelsPerUnit(sheetPath, 400f);

            var enemySprites = LoadSpritesOrdered(sheetPath)
                .Where(s => s.rect.width >= 64 && s.rect.height >= 64)
                .ToList();

            if (enemySprites.Count < EnemyCycleFrameCount)
                throw new System.InvalidOperationException(
                    $"Expected at least {EnemyCycleFrameCount} enemy sprites in enemy_sheet.png, got {enemySprites.Count}.");

            var (enemyIdle, enemyWalk, enemyAttack, enemyDeath) = BuildEnemyClipsFromFrames("enemy", enemySprites);

            AssignClipsToExistingController(
                "Assets/Animation/Controllers/AC_Enemy.controller",
                enemyIdle,
                enemyWalk,
                enemyAttack,
                deathClip: enemyDeath);

            var defaultSprite = enemySprites[0];
            var tierBindings = new[]
            {
                ("level1", "Assets/Animation/Controllers/AC_Enemy_Level1.controller", "Level1Enemy.prefab"),
                ("level2", "Assets/Animation/Controllers/AC_Enemy_Level2.controller", "Level2Enemy.prefab"),
                ("level3", "Assets/Animation/Controllers/AC_Enemy_Level3.controller", "Level3Enemy.prefab"),
            };

            foreach (var (tierId, controllerPath, prefabFile) in tierBindings)
            {
                var (tierIdle, tierWalk, tierAttack, tierDeath) = BuildEnemyClipsFromFrames($"enemy_{tierId}", enemySprites);
                AssignClipsToExistingController(controllerPath, tierIdle, tierWalk, tierAttack, deathClip: tierDeath);
                SetupEnemyPrefab("Assets/Prefabs/Enemies/" + prefabFile, defaultSprite, controllerPath);
                SetupEnemyPrefab("Assets/Prefabs/Enemy/" + prefabFile, defaultSprite, controllerPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[CharacterAnimationPipeline] Bound enemy_sheet sprites: count={enemySprites.Count}.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[CharacterAnimationPipeline] BindArtFolderEnemySprites: " + ex);
            if (Application.isBatchMode)
                EditorApplication.Exit(1);
        }
    }

    /// <summary>
    /// Rebuilds per-tier enemy clips/controllers from dev sprite sheets and updates Level1/2/3 prefabs.
    /// Unity -batchmode -quit -executeMethod CharacterAnimationPipeline.BindDevTierEnemySprites
    /// </summary>
    public static void BindDevTierEnemySprites()
    {
        try
        {
            EnsureFolders();

            BindDevTierEnemy(
                "Assets/Sprites/IMG_0603.png",
                "level1",
                "Assets/Animation/Controllers/AC_Enemy_Level1.controller",
                "Assets/Prefabs/Enemy/Level1Enemy.prefab");

            BindDevTierEnemy(
                "Assets/Sprites/IMG_0652.png",
                "level2",
                "Assets/Animation/Controllers/AC_Enemy_Level2.controller",
                "Assets/Prefabs/Enemy/Level2Enemy.prefab");

            BindDevTierEnemy(
                "Assets/Sprites/final binal/IMG_0639.png",
                "level3",
                "Assets/Animation/Controllers/AC_Enemy_Level3.controller",
                "Assets/Prefabs/Enemy/Level3Enemy.prefab");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CharacterAnimationPipeline] Bound dev-tier enemy sprites for Level1/2/3.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[CharacterAnimationPipeline] BindDevTierEnemySprites: " + ex);
            if (Application.isBatchMode)
                EditorApplication.Exit(1);
        }
    }

    static void BindDevTierEnemy(
        string texturePath,
        string tierId,
        string controllerPath,
        string prefabPath)
    {
        if (!File.Exists(texturePath))
            throw new FileNotFoundException("Missing " + texturePath);

        var sprites = LoadSpritesOrdered(texturePath)
            .Where(s => s.rect.width >= 64 && s.rect.height >= 64)
            .ToList();

        if (sprites.Count == 0)
            throw new System.InvalidOperationException("No usable sprites in " + texturePath);

        var characterSprites = FilterCharacterSprites(sprites);
        var primarySprite = PickPrimarySprite(characterSprites);

        var (idleClip, walkClip, attackClip, deathClip) = BuildEnemyClipsFromFrames($"enemy_{tierId}", characterSprites);

        EnsureEnemyTierController(controllerPath, idleClip, walkClip, attackClip, deathClip);
        SetupEnemyPrefab(prefabPath, primarySprite, controllerPath);
    }

    static List<Sprite> TakeEnemyCycleFrames(IReadOnlyList<Sprite> sprites)
    {
        var count = System.Math.Min(EnemyCycleFrameCount, sprites.Count);
        return sprites.Take(count).ToList();
    }

    static int[] EnemyCycleFrameIndices(IReadOnlyList<Sprite> sprites)
    {
        var count = System.Math.Min(EnemyCycleFrameCount, sprites.Count);
        return Enumerable.Range(0, count).ToArray();
    }

    static (AnimationClip idle, AnimationClip walk, AnimationClip attack, AnimationClip death) BuildEnemyClipsFromFrames(
        string clipIdPrefix,
        IReadOnlyList<Sprite> sprites)
    {
        var cycle = TakeEnemyCycleFrames(sprites);
        var idle = BuildOrUpdateSpriteClipFromFrames(
            $"Assets/Animation/Clips/{clipIdPrefix}_idle.anim",
            cycle,
            EnemyIdleCycleDuration,
            loop: true);
        var walk = BuildOrUpdateSpriteClipFromFrames(
            $"Assets/Animation/Clips/{clipIdPrefix}_walk.anim",
            cycle,
            EnemyWalkCycleDuration,
            loop: true);
        var attackFrames = cycle.Count >= 3
            ? cycle.GetRange(cycle.Count - 3, 3)
            : cycle;
        var attack = BuildOrUpdateSpriteClipFromFrames(
            $"Assets/Animation/Clips/{clipIdPrefix}_attack.anim",
            attackFrames,
            0.25f,
            loop: false);
        var death = BuildOrUpdateSpriteClipFromFrames(
            $"Assets/Animation/Clips/{clipIdPrefix}_death.anim",
            new List<Sprite> { cycle[cycle.Count - 1] },
            0.4f,
            loop: false);
        return (idle, walk, attack, death);
    }

    static List<Sprite> FilterCharacterSprites(IReadOnlyList<Sprite> sprites)
    {
        const float minHeight = 200f;
        var tall = sprites.Where(s => s.rect.height >= minHeight).ToList();
        return tall.Count > 0 ? tall : sprites.ToList();
    }

    static Sprite PickPrimarySprite(IReadOnlyList<Sprite> sprites)
    {
        var best = sprites[0];
        var bestArea = best.rect.width * best.rect.height;
        for (var i = 1; i < sprites.Count; i++)
        {
            var area = sprites[i].rect.width * sprites[i].rect.height;
            if (area <= bestArea)
                continue;
            bestArea = area;
            best = sprites[i];
        }

        return best;
    }

    static void EnsureEnemyTierController(
        string controllerPath,
        AnimationClip idle,
        AnimationClip walk,
        AnimationClip attack,
        AnimationClip death)
    {
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) == null)
        {
            BuildController(controllerPath, idle, walk, attack, includeDeath: true, deathClip: death);
            return;
        }

        AssignClipsToExistingController(controllerPath, idle, walk, attack, deathClip: death);
    }

    static void EnsureEnemySheetSpriteSheet(string assetPath)
    {
        var existing = LoadSpritesOrdered(assetPath);
        if (existing.Count >= 7 && existing.All(s => s.rect.width > 64 && s.rect.height > 64))
            return;

        ApplyManualSpriteRects(assetPath, "enemy_sheet", new[]
        {
            new Rect(94f, 1438f, 343f, 329f),
            new Rect(563f, 1434f, 331f, 330f),
            new Rect(1053f, 1435f, 316f, 328f),
            new Rect(1589f, 1436f, 310f, 324f),
            new Rect(1027f, 763f, 327f, 318f),
            new Rect(563f, 754f, 320f, 315f),
            new Rect(1583f, 744f, 333f, 320f),
            new Rect(108f, 741f, 313f, 319f),
            new Rect(814f, 125f, 338f, 321f),
        }, pixelsPerUnit: 400f);
    }

    static void EnsureSpriteSheetPixelsPerUnit(string assetPath, float pixelsPerUnit)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
            throw new System.InvalidOperationException("No TextureImporter for " + assetPath);

        if (Mathf.Approximately(importer.spritePixelsPerUnit, pixelsPerUnit))
            return;

        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.SaveAndReimport();
    }

    static void EnsurePlayerWalkSpriteSheet(string assetPath)
    {
        var existing = LoadSpritesOrdered(assetPath);
        if (existing.Count > 0 && existing.All(s => s.rect.width > 64 && s.rect.height > 64))
            return;

        ApplyManualSpriteRects(assetPath, "player_walk", new[]
        {
            new Rect(195f, 1096f, 388f, 859f),
            new Rect(830f, 1090f, 388f, 859f),
            new Rect(1460f, 1079f, 388f, 859f),
            new Rect(833f, 50f, 388f, 860f),
        });
    }

    static void ApplyManualSpriteRects(string assetPath, string prefix, Rect[] rects, float pixelsPerUnit = 100f)
    {
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
            throw new System.InvalidOperationException("No TextureImporter for " + assetPath);

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.spritePixelsPerUnit = pixelsPerUnit;

        var spriteRects = new SpriteRect[rects.Length];
        for (var i = 0; i < rects.Length; i++)
        {
            spriteRects[i] = new SpriteRect
            {
                name = $"{prefix}_{i}",
                rect = rects[i],
                alignment = SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f),
                border = Vector4.zero,
                spriteID = GUID.Generate()
            };
        }

        ApplySpriteRectsViaDataProvider(importer, spriteRects);
    }

    public static void GenerateAll()
    {
        try
        {
            EnsureFolders();
            WritePlaceholderSheets();
            AssetDatabase.Refresh();

            var playerSprites = ImportAndSlice("Assets/Art/Characters/Player/player_sheet.png", "p");
            var enemySprites = ImportAndSlice("Assets/Art/Characters/Enemy/enemy_sheet.png", "e");

            var playerIdle = BuildSpriteClip("Assets/Animation/Clips/player_idle.anim", playerSprites, new[] { 0, 1 }, 0.5f, loop: true);
            var playerWalk = BuildSpriteClip("Assets/Animation/Clips/player_walk.anim", playerSprites, new[] { 2, 3, 4 }, 0.35f, loop: true);
            var playerAttack = BuildSpriteClip("Assets/Animation/Clips/player_attack.anim", playerSprites, new[] { 5, 6 }, 0.25f, loop: false);

            var enemyCycleIndices = EnemyCycleFrameIndices(enemySprites);
            var enemyIdle = BuildSpriteClip(
                "Assets/Animation/Clips/enemy_idle.anim", enemySprites, enemyCycleIndices, EnemyIdleCycleDuration, loop: true);
            var enemyWalk = BuildSpriteClip(
                "Assets/Animation/Clips/enemy_walk.anim", enemySprites, enemyCycleIndices, EnemyWalkCycleDuration, loop: true);
            var enemyAttack = BuildSpriteClip(
                "Assets/Animation/Clips/enemy_attack.anim",
                enemySprites,
                enemyCycleIndices.Length >= 3
                    ? new[] { enemyCycleIndices[^3], enemyCycleIndices[^2], enemyCycleIndices[^1] }
                    : enemyCycleIndices,
                0.25f,
                loop: false);
            var enemyDeath = BuildSpriteClip(
                "Assets/Animation/Clips/enemy_death.anim",
                enemySprites,
                new[] { enemyCycleIndices[^1] },
                0.4f,
                loop: false);

            BuildController(
                "Assets/Animation/Controllers/AC_Player.controller",
                playerIdle,
                playerWalk,
                playerAttack,
                includeDeath: false);

            BuildController(
                "Assets/Animation/Controllers/AC_Enemy.controller",
                enemyIdle,
                enemyWalk,
                enemyAttack,
                includeDeath: true,
                deathClip: enemyDeath);

            SetupPlayerInSampleScene();
            SetupEnemyPrefab("Assets/Prefabs/Enemy/Level1Enemy.prefab");
            SetupEnemyPrefab("Assets/Prefabs/Enemy/Level2Enemy.prefab");
            SetupEnemyPrefab("Assets/Prefabs/Enemy/Level3Enemy.prefab");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CharacterAnimationPipeline] Done.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[CharacterAnimationPipeline] " + ex);
            EditorApplication.Exit(1);
        }
    }

    /// <summary>
    /// Rebuilds player/enemy sprite clips from production textures and assigns motions on existing AC_Player / AC_Enemy.
    /// Does not delete .anim assets (preserves Animator references). Skips bogus tiny slices on attack sheets.
    /// Run: Unity -batchmode -quit -executeMethod CharacterAnimationPipeline.BindProductionCharacterSprites
    /// </summary>
    public static void BindProductionCharacterSprites()
    {
        try
        {
            EnsureFolders();

            var playerIdleSprites = LoadSpritesOrdered("Assets/Art/Characters/Player/Player_idle.png");
            var playerWalkSprites = LoadSpritesOrdered("Assets/Art/Characters/Player/Player_Walk.png");
            var playerAttackSprites = LoadSpritesOrdered("Assets/Art/Characters/Player/Player_attack.png")
                .Where(s => s.rect.width >= 32 && s.rect.height >= 32)
                .ToList();

            if (playerIdleSprites.Count == 0)
                throw new System.InvalidOperationException("No sprites in Player_idle.png (slice / reimport).");
            if (playerWalkSprites.Count == 0)
                throw new System.InvalidOperationException("No sprites in Player_Walk.png (slice / reimport).");
            if (playerAttackSprites.Count == 0)
                throw new System.InvalidOperationException(
                    "No usable attack sprites in Player_attack.png (after filtering tiny slices).");

            var enemySprites = LoadSpritesOrdered("Assets/Art/Characters/Enemy/Enemy1.png");
            if (enemySprites.Count < 9)
                throw new System.InvalidOperationException(
                    $"Enemy1.png: expected at least 9 sprites, got {enemySprites.Count}.");

            var playerIdle = BuildOrUpdateSpriteClipFromFrames(
                "Assets/Animation/Clips/player_idle.anim", playerIdleSprites, 0.5f, loop: true);
            var playerWalk = BuildOrUpdateSpriteClipFromFrames(
                "Assets/Animation/Clips/player_walk.anim", playerWalkSprites, 0.35f, loop: true);
            var playerAttack = BuildOrUpdateSpriteClipFromFrames(
                "Assets/Animation/Clips/player_attack.anim", playerAttackSprites, 0.25f, loop: false);

            var (enemyIdle, enemyWalk, enemyAttack, enemyDeath) = BuildEnemyClipsFromFrames("enemy", enemySprites);

            AssignClipsToExistingController(
                "Assets/Animation/Controllers/AC_Player.controller",
                playerIdle,
                playerWalk,
                playerAttack,
                deathClip: null);
            AssignClipsToExistingController(
                "Assets/Animation/Controllers/AC_Enemy.controller",
                enemyIdle,
                enemyWalk,
                enemyAttack,
                deathClip: enemyDeath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CharacterAnimationPipeline] BindProductionCharacterSprites done.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[CharacterAnimationPipeline] BindProductionCharacterSprites: " + ex);
            if (Application.isBatchMode)
                EditorApplication.Exit(1);
        }
    }

    [MenuItem("Tools/Elemental/Bind Enemy Sheet Sprites")]
    static void BindArtFolderEnemySpritesMenu()
    {
        BindArtFolderEnemySprites();
    }

    [MenuItem("Tools/Elemental/Bind Production Character Sprites")]
    static void BindProductionCharacterSpritesMenu()
    {
        BindProductionCharacterSprites();
    }

    static void EnsureFolders()
    {
        foreach (var dir in new[]
                 {
                     "Assets/Art/Characters/Player",
                     "Assets/Art/Characters/Enemy",
                     "Assets/Animation/Clips",
                     "Assets/Animation/Controllers"
                 })
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
    }

    static void WritePlaceholderSheets()
    {
        WriteSheet(Path.Combine("Assets/Art/Characters/Player", "player_sheet.png"), new Color(0.35f, 0.45f, 1f));
        WriteSheet(Path.Combine("Assets/Art/Characters/Enemy", "enemy_sheet.png"), new Color(1f, 0.55f, 0.15f));
    }

    static void WriteSheet(string assetPath, Color baseTint)
    {
        var tex = new Texture2D(FrameW * FrameCount, FrameH, TextureFormat.RGBA32, false);
        for (var f = 0; f < FrameCount; f++)
        {
            var tint = Color.Lerp(baseTint, baseTint * 1.35f, f / (float)(FrameCount - 1));
            for (var x = 0; x < FrameW; x++)
            for (var y = 0; y < FrameH; y++)
                tex.SetPixel(f * FrameW + x, y, tint);
        }

        tex.Apply();
        File.WriteAllBytes(assetPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    static void ApplySpriteRectsViaDataProvider(TextureImporter importer, SpriteRect[] spriteRects)
    {
        var factory = new SpriteDataProviderFactories();
        factory.Init();
        var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
        if (dataProvider == null)
            throw new System.InvalidOperationException("No ISpriteEditorDataProvider for " + importer.assetPath);

        dataProvider.InitSpriteEditorDataProvider();
        dataProvider.SetSpriteRects(spriteRects);

        var nameIdProvider = dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        if (nameIdProvider != null)
        {
            var pairs = new List<SpriteNameFileIdPair>(spriteRects.Length);
            foreach (var sr in spriteRects)
                pairs.Add(new SpriteNameFileIdPair(sr.name, sr.spriteID));
            nameIdProvider.SetNameFileIdPairs(pairs.ToArray());
        }

        dataProvider.Apply();
        importer.SaveAndReimport();
    }

    static Sprite[] ImportAndSlice(string assetPath, string prefix)
    {
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
            throw new System.InvalidOperationException("No TextureImporter for " + assetPath);

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;

        var spriteRects = new SpriteRect[FrameCount];
        for (var i = 0; i < FrameCount; i++)
        {
            spriteRects[i] = new SpriteRect
            {
                name = $"{prefix}_{i}",
                rect = new Rect(i * FrameW, 0, FrameW, FrameH),
                alignment = SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f),
                border = Vector4.zero,
                spriteID = UnityEditor.GUID.Generate()
            };
        }

        ApplySpriteRectsViaDataProvider(importer, spriteRects);

        var sprites = new List<Sprite>();
        var objs = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        foreach (var o in objs)
        {
            if (o is Sprite s && s.name.StartsWith(prefix + "_", System.StringComparison.Ordinal))
                sprites.Add(s);
        }

        sprites.Sort((a, b) =>
        {
            var ia = int.Parse(a.name.Substring(prefix.Length + 1));
            var ib = int.Parse(b.name.Substring(prefix.Length + 1));
            return ia.CompareTo(ib);
        });

        if (sprites.Count != FrameCount)
            throw new System.InvalidOperationException($"Expected {FrameCount} sprites in {assetPath}, got {sprites.Count}");

        return sprites.ToArray();
    }

    static AnimationClip BuildSpriteClip(string path, IReadOnlyList<Sprite> sprites, int[] frameIndices, float totalDuration, bool loop)
    {
        if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
            AssetDatabase.DeleteAsset(path);

        var clip = new AnimationClip { name = Path.GetFileNameWithoutExtension(path) };
        var step = frameIndices.Length > 0 ? totalDuration / frameIndices.Length : totalDuration;
        var keys = new ObjectReferenceKeyframe[frameIndices.Length];
        for (var i = 0; i < frameIndices.Length; i++)
        {
            keys[i] = new ObjectReferenceKeyframe
            {
                time = i * step,
                value = sprites[frameIndices[i]]
            };
        }

        var binding = EditorCurveBinding.PPtrCurve(string.Empty, typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        clip.wrapMode = loop ? WrapMode.Loop : WrapMode.ClampForever;
        AssetDatabase.CreateAsset(clip, path);
        var loaded = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        var settings = AnimationUtility.GetAnimationClipSettings(loaded);
        settings.loopTime = loop;
        settings.stopTime = totalDuration;
        AnimationUtility.SetAnimationClipSettings(loaded, settings);
        return loaded;
    }

    static List<Sprite> LoadSpritesOrdered(string textureAssetPath)
    {
        if (AssetDatabase.LoadAssetAtPath<Texture2D>(textureAssetPath) == null)
            throw new System.IO.FileNotFoundException("Texture not found: " + textureAssetPath);

        var list = new List<Sprite>();
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath(textureAssetPath))
        {
            if (o is Sprite s)
                list.Add(s);
        }

        list.Sort((a, b) => SpriteNameFrameIndex(a.name).CompareTo(SpriteNameFrameIndex(b.name)));
        return list;
    }

    static int SpriteNameFrameIndex(string spriteName)
    {
        var i = spriteName.LastIndexOf('_');
        if (i >= 0 && int.TryParse(spriteName.Substring(i + 1), out var n))
            return n;
        return 0;
    }

    /// <summary>
    /// Updates an existing clip in-place so AnimatorController fileIDs stay valid.
    /// </summary>
    static AnimationClip BuildOrUpdateSpriteClipFromFrames(
        string path,
        IReadOnlyList<Sprite> framesOrdered,
        float totalDuration,
        bool loop)
    {
        if (framesOrdered == null || framesOrdered.Count == 0)
            throw new System.InvalidOperationException("No frames for clip: " + path);

        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null)
        {
            clip = new AnimationClip { name = Path.GetFileNameWithoutExtension(path) };
            AssetDatabase.CreateAsset(clip, path);
            clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        }

        var step = totalDuration / framesOrdered.Count;
        var keys = new ObjectReferenceKeyframe[framesOrdered.Count];
        for (var i = 0; i < framesOrdered.Count; i++)
        {
            keys[i] = new ObjectReferenceKeyframe
            {
                time = i * step,
                value = framesOrdered[i]
            };
        }

        var binding = EditorCurveBinding.PPtrCurve(string.Empty, typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        clip.wrapMode = loop ? WrapMode.Loop : WrapMode.ClampForever;
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        settings.stopTime = totalDuration;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        EditorUtility.SetDirty(clip);
        return clip;
    }

    static void AssignClipsToExistingController(
        string controllerPath,
        AnimationClip idle,
        AnimationClip walk,
        AnimationClip attack,
        AnimationClip deathClip)
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
            throw new System.InvalidOperationException("AnimatorController not found: " + controllerPath);

        var sm = controller.layers[0].stateMachine;
        foreach (var child in sm.states)
        {
            var st = child.state;
            switch (st.name)
            {
                case "Idle":
                    st.motion = idle;
                    break;
                case "Walk":
                    st.motion = walk;
                    break;
                case "Attack":
                    st.motion = attack;
                    break;
                case "Death":
                    if (deathClip != null)
                        st.motion = deathClip;
                    break;
            }
        }

        EditorUtility.SetDirty(controller);
    }

    static void BuildController(
        string path,
        AnimationClip idle,
        AnimationClip walk,
        AnimationClip attack,
        bool includeDeath,
        AnimationClip deathClip = null)
    {
        if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
            AssetDatabase.DeleteAsset(path);

        var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        controller.AddParameter(AnimationParams.Speed, AnimatorControllerParameterType.Float);
        controller.AddParameter(AnimationParams.Attack, AnimatorControllerParameterType.Trigger);
        if (includeDeath)
            controller.AddParameter(AnimationParams.Die, AnimatorControllerParameterType.Trigger);

        var root = controller.layers[0].stateMachine;
        var idleSt = AddState(root, "Idle", idle, new Vector3(250, 0, 0));
        root.defaultState = idleSt;
        var walkSt = AddState(root, "Walk", walk, new Vector3(250, 80, 0));
        var attackSt = AddState(root, "Attack", attack, new Vector3(500, 40, 0));

        AnimatorState deathSt = null;
        if (includeDeath && deathClip != null)
            deathSt = AddState(root, "Death", deathClip, new Vector3(500, -80, 0));

        AddSpeedTransition(idleSt, walkSt, AnimatorConditionMode.Greater, 0.08f);
        AddSpeedTransition(walkSt, idleSt, AnimatorConditionMode.Less, 0.06f);

        AddTriggerTransition(idleSt, attackSt, AnimationParams.Attack);
        AddTriggerTransition(walkSt, attackSt, AnimationParams.Attack);

        AddExitAttack(attackSt, idleSt, walkSt);

        if (includeDeath && deathSt != null)
        {
            var any = root.AddAnyStateTransition(deathSt);
            any.hasExitTime = false;
            any.duration = 0.05f;
            any.canTransitionToSelf = false;
            any.interruptionSource = TransitionInterruptionSource.None;
            any.AddCondition(AnimatorConditionMode.If, 0f, AnimationParams.Die);
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
    }

    static AnimatorState AddState(AnimatorStateMachine sm, string name, Motion motion, Vector3 pos)
    {
        var st = sm.AddState(name, pos);
        st.motion = motion;
        st.writeDefaultValues = true;
        return st;
    }

    static void AddSpeedTransition(AnimatorState from, AnimatorState to, AnimatorConditionMode mode, float threshold)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = false;
        t.duration = 0.08f;
        t.AddCondition(mode, threshold, AnimationParams.Speed);
    }

    static void AddTriggerTransition(AnimatorState from, AnimatorState to, string trigger)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = false;
        t.duration = 0.05f;
        t.AddCondition(AnimatorConditionMode.If, 0f, trigger);
    }

    static void AddExitAttack(AnimatorState attack, AnimatorState idle, AnimatorState walk)
    {
        var toIdle = attack.AddTransition(idle);
        toIdle.hasExitTime = true;
        toIdle.exitTime = 0.85f;
        toIdle.duration = 0.08f;
        toIdle.AddCondition(AnimatorConditionMode.Less, 0.07f, AnimationParams.Speed);

        var toWalk = attack.AddTransition(walk);
        toWalk.hasExitTime = true;
        toWalk.exitTime = 0.85f;
        toWalk.duration = 0.08f;
        toWalk.AddCondition(AnimatorConditionMode.Greater, 0.09f, AnimationParams.Speed);
    }

    static void AssignDefaultPlayerVisualSprite(Sprite sprite)
    {
        if (sprite == null)
            return;

        var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        GameObject player = null;
        foreach (var go in scene.GetRootGameObjects())
        {
            if (go.CompareTag("Player"))
            {
                player = go;
                break;
            }
        }

        if (player == null)
            return;

        var visual = player.transform.Find("Visual");
        if (visual == null)
            return;

        var sr = visual.GetComponent<SpriteRenderer>();
        if (sr == null)
            return;

        sr.sprite = sprite;
        var scale = ResolveVisualDisplayScale(sprite);
        visual.localScale = new Vector3(scale, scale, 1f);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
    }

    static void SetupPlayerInSampleScene()
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");

        var roots = scene.GetRootGameObjects();
        GameObject player = null;
        foreach (var go in roots)
        {
            if (go.CompareTag("Player"))
            {
                player = go;
                break;
            }
        }

        if (player == null)
        {
            Debug.LogWarning("[CharacterAnimationPipeline] No Player in SampleScene.");
            return;
        }

        EnsureVisualAnimator(
            player,
            "Assets/Animation/Controllers/AC_Player.controller",
            addPlayerCharacterAnimation: true);

        var weapon = player.GetComponent<PlayerDefaultWeapon>();
        if (weapon != null)
        {
            var so = new SerializedObject(weapon);
            var anim = player.GetComponent<PlayerCharacterAnimation>();
            so.FindProperty("characterAnimation").objectReferenceValue = anim;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
    }

    static void SetupEnemyPrefab(
        string prefabPath,
        Sprite defaultVisualSprite = null,
        string controllerPath = "Assets/Animation/Controllers/AC_Enemy.controller")
    {
        var root = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            EnsureVisualAnimator(
                root,
                controllerPath,
                addPlayerCharacterAnimation: false);

            var visual = root.transform.Find("Visual");
            if (visual != null && defaultVisualSprite != null)
            {
                var sr = visual.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = defaultVisualSprite;
                    sr.drawMode = SpriteDrawMode.Simple;
                    sr.spriteSortPoint = SpriteSortPoint.Center;
                }

                var scale = ResolveVisualDisplayScale(defaultVisualSprite);
                visual.localScale = new Vector3(scale, scale, 1f);
            }

            var ai = root.GetComponent<EnemyAI>();
            if (ai != null)
            {
                var so = new SerializedObject(ai);
                so.FindProperty("characterAnimation").objectReferenceValue =
                    root.GetComponent<EnemyCharacterAnimation>();
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static float ResolveVisualDisplayScale(Sprite sprite)
    {
        if (sprite == null)
            return VisualDisplayScale;

        var worldHeight = sprite.rect.height / sprite.pixelsPerUnit;
        if (worldHeight > 2f)
            return VisualDisplayScale;
        if (worldHeight > 0.5f)
            return 1f;
        return VisualDisplayScale;
    }

    static void EnsureVisualAnimator(GameObject root, string controllerPath, bool addPlayerCharacterAnimation)
    {
        var existingVisual = root.transform.Find("Visual");
        GameObject visualGo;
        SpriteRenderer sr;
        Animator animator;

        if (existingVisual != null)
        {
            visualGo = existingVisual.gameObject;
            sr = visualGo.GetComponent<SpriteRenderer>();
            animator = visualGo.GetComponent<Animator>();
            if (sr == null)
                sr = visualGo.AddComponent<SpriteRenderer>();
            if (animator == null)
                animator = visualGo.AddComponent<Animator>();
        }
        else
        {
            var oldSr = root.GetComponent<SpriteRenderer>();
            visualGo = new GameObject("Visual");
            visualGo.transform.SetParent(root.transform, false);
            visualGo.transform.localPosition = Vector3.zero;
            visualGo.transform.localRotation = Quaternion.identity;
            visualGo.transform.localScale = new Vector3(
                ResolveVisualDisplayScale(oldSr != null ? oldSr.sprite : null),
                ResolveVisualDisplayScale(oldSr != null ? oldSr.sprite : null),
                1f);

            sr = visualGo.AddComponent<SpriteRenderer>();
            animator = visualGo.AddComponent<Animator>();

            if (oldSr != null)
            {
                sr.sprite = oldSr.sprite;
                sr.color = oldSr.color;
                sr.flipX = oldSr.flipX;
                sr.flipY = oldSr.flipY;
                sr.sharedMaterial = oldSr.sharedMaterial;
                sr.sortingLayerID = oldSr.sortingLayerID;
                sr.sortingOrder = oldSr.sortingOrder;
                sr.maskInteraction = oldSr.maskInteraction;
                sr.drawMode = oldSr.drawMode;
                sr.size = oldSr.size;
                sr.spriteSortPoint = oldSr.spriteSortPoint;
                Object.DestroyImmediate(oldSr, true);
            }
        }

        var displayScale = ResolveVisualDisplayScale(sr.sprite);
        visualGo.transform.localScale = new Vector3(displayScale, displayScale, 1f);
        visualGo.layer = root.layer;

        if (sr.sprite != null)
        {
            sr.drawMode = SpriteDrawMode.Simple;
            sr.spriteSortPoint = SpriteSortPoint.Center;
        }

        var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;

        var rb = root.GetComponent<Rigidbody2D>();

        if (addPlayerCharacterAnimation)
        {
            var pca = root.GetComponent<PlayerCharacterAnimation>();
            if (pca == null)
                pca = root.AddComponent<PlayerCharacterAnimation>();
            var so = new SerializedObject(pca);
            so.FindProperty("rb").objectReferenceValue = rb;
            so.FindProperty("animator").objectReferenceValue = animator;
            so.FindProperty("spriteRenderer").objectReferenceValue = sr;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        else
        {
            var eca = root.GetComponent<EnemyCharacterAnimation>();
            if (eca == null)
                eca = root.AddComponent<EnemyCharacterAnimation>();
            var so = new SerializedObject(eca);
            so.FindProperty("rb").objectReferenceValue = rb;
            so.FindProperty("animator").objectReferenceValue = animator;
            so.FindProperty("spriteRenderer").objectReferenceValue = sr;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        var vfxPresenter = root.GetComponent<ElementalStatusVfxPresenter>();
        if (vfxPresenter != null)
        {
            var vfxSo = new SerializedObject(vfxPresenter);
            vfxSo.FindProperty("optionalAnchor").objectReferenceValue = visualGo.transform;
            vfxSo.FindProperty("vfxLocalOffset").vector3Value = new Vector3(0f, -0.6f, 0f);
            vfxSo.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
