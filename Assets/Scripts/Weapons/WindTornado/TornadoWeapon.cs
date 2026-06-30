using UnityEngine;

public class TornadoWeapon : WeaponBehavior
{
    [SerializeField] private Tornado tornadoPrefab;
    public override bool TryExecute(Enemy target, WeaponLevelData data, WeaponSystemContext ctx, WeaponDefinition definition)
    {
        int count = Mathf.Max(1, data.projectileCount);

        for (int i = 0; i < count; i++)
        {
            Vector2 spawnPos = GetRandomPositionInCamera();

            Tornado tornado = Instantiate(tornadoPrefab, spawnPos, Quaternion.identity);

            tornado.Init(data.damage, data.speed, data.range, data.statusDuration, data.lifetime, definition.appliedStatus, ctx.StatusSystem);
        }

        return true;
    }

    private Vector2 GetRandomPositionInCamera()
    {
        Camera mainCam = Camera.main;

        Vector3 bottomLeft = mainCam.ViewportToWorldPoint(new Vector3(0, 0, -mainCam.transform.position.z));
        Vector3 topRight = mainCam.ViewportToWorldPoint(new Vector3(1, 1, -mainCam.transform.position.z));

        return new Vector2(
            Random.Range(bottomLeft.x, topRight.x),
            Random.Range(bottomLeft.y, topRight.y));
    }
}
