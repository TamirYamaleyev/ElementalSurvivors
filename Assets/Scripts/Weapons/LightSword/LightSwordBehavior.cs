using UnityEngine;

public class LightSwordBehavior : WeaponBehavior
{
    [SerializeField] AudioClip sfx;

    [SerializeField] private LightSword swordPrefab;
    [SerializeField] private float maxSpawnDistance = 5f;

    public override bool TryExecute(Enemy target, WeaponLevelData data, WeaponSystemContext ctx, WeaponDefinition definition)
    {
        Vector2 playerPos = ctx.PlayerTransformPoint.position;
        Vector2 mousePos = ctx.AimDirection.MouseWorldPosition;

        Vector2 delta = mousePos - playerPos;
        Vector2 dir = delta.sqrMagnitude < 0.0001f ? Vector2.right : delta.normalized;

        Vector2 spawnPos = playerPos + dir * maxSpawnDistance;

        Vector2 slashDir = delta.normalized;

        Debug.DrawLine(playerPos, mousePos, Color.red, 1f);
        Debug.DrawLine(playerPos, spawnPos, Color.green, 1f);

        var sword = Instantiate(swordPrefab, spawnPos, Quaternion.identity, ctx.PlayerTransformPoint);

        AudioManager.Instance.PlaySfx(sfx, 0.75f);

        sword.transform.localScale = new Vector3(data.width, data.height, 1f);

        sword.Init(
            data.damage,
            data.lifetime,
            definition.appliedStatus,
            data.statusDuration,
            ctx.StatusSystem,
            data.visualSpriteArr,
            slashDir
        );

        return true;
    }
}
