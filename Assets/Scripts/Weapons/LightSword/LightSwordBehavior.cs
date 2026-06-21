using Unity.VisualScripting;
using UnityEngine;

public class LightSwordBehavior : WeaponBehavior
{
    [SerializeField] private Camera mainCam;

    [SerializeField] private LightSword swordPrefab;

    public override bool TryExecute(Enemy target, WeaponLevelData data, WeaponSystemContext ctx, WeaponDefinition definition)
    {
        Transform player = ctx.PlayerTransformPoint;
        Vector2 playerPos = player.position;

        Vector2 mousePos = mainCam.ScreenToWorldPoint(ctx.AimDirection.LastDirection);

        Vector2 direction = mousePos - playerPos;

        if (direction.sqrMagnitude > data.range * data.range)
        {
            direction = direction.normalized * data.range;
        }

        Vector2 spawnpos = playerPos + direction;

        var sword = Instantiate(swordPrefab, spawnpos, Quaternion.identity, player);

        sword.transform.localScale = new Vector3(data.width, data.height, 1f);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        sword.Init(
            data.damage,
            data.lifetime,
            definition.appliedStatus,
            data.statusDuration,
            ctx.StatusSystem,
            data.visualSprite
        );

        return true;
    }
}
