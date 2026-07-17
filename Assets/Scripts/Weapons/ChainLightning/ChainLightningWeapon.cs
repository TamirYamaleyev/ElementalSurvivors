using UnityEngine;

public class ChainLightningWeapon : WeaponBehavior
{
    [SerializeField] private ChaingLightningVisual visualPrefab;
    [SerializeField] private float visualLifetime = 0.1f;

    public override bool TryExecute(Enemy target, WeaponLevelData data, WeaponSystemContext ctx, WeaponDefinition definition)
    {
        visualPrefab.SwapSpriteSheet(data.visualSpriteArr);

        Vector2 origin = ctx.ProjectileSpawnPoint.position;

        var targets = ctx.Targeting.GetChainTargets(origin, data.projectileCount, data.range);

        if (targets == null || targets.Count == 0)
            return false;

        float damage = ctx.PlayerStats != null
            ? CombatStatResolver.ScaleDamage(data.damage, ctx.PlayerStats.Current)
            : data.damage;

        Vector2 previousPoint = origin;

        foreach (var enemy in targets)
        {
            if (enemy == null)
                continue;

            Vector2 hitPoint = enemy.transform.position;

            var visual = Instantiate(visualPrefab);

            visual.Initialize(previousPoint, hitPoint, data.visualSpriteArr[0], visualLifetime);

            CombatHitUtility.ApplyStatusThenDamage(
                enemy,
                ctx.StatusSystem,
                definition.appliedStatus,
                data.statusDuration,
                damage);

            previousPoint = hitPoint;
        }

        return true;
    }
}
