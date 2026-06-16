using UnityEngine;
using UnityEngine.InputSystem;

public class BoomerangController : MonoBehaviour
{
    [SerializeField] private Camera mainCam;
    [SerializeField] private BoomerangWeapon boomerangPrefab;
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private float spawnOffset = 1f;

    [SerializeField] private int[] boomerangsPerLevel = { 1, 2, 3 };
    [SerializeField] private float spreadAngle = 25f;

    private int currentLevel = 0;
    private int BoomerangCount => boomerangsPerLevel[currentLevel];

    private float timer;
    private Transform player;

    private Vector2 lastDirection = Vector2.right;

    public void LevelUp()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        EnsureInitialized();

        if (currentLevel >= boomerangsPerLevel.Length - 1) return;
        currentLevel++;

        spreadAngle = 20f + currentLevel * 10f;
    }

    void Update()
    {
        if (player == null)
            return;

        UpdateDirection();

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            Fire();
            timer = cooldown;
        }
    }

    private void EnsureInitialized()
    {
        if (player != null)
            return;

        player = PlayerController.Instance;
        timer = 0f;
    }

    private void UpdateDirection()
    {
        if (Mouse.current == null) return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        Vector2 dir = mouseWorldPos - player.position;

        if (dir.sqrMagnitude > 0.0001f)
            lastDirection = dir.normalized;
    }

    private void Fire()
    {
        if (boomerangPrefab == null)
            return;

        int count = BoomerangCount;

        float baseAngle = Mathf.Atan2(lastDirection.y, lastDirection.x) * Mathf.Rad2Deg;

        for (int i = 0; i < count; i++)
        {
            float t = count == 1 ? 0.5f : (float)i / (count - 1);
            float angleOffset = Mathf.Lerp(-spreadAngle / 2f, spreadAngle / 2f, t);
            angleOffset += Random.Range(-2f, 2f);

            float finalAngle = baseAngle + angleOffset;

            Vector2 dir = new Vector2(
                Mathf.Cos(finalAngle * Mathf.Deg2Rad),
                Mathf.Sin(finalAngle * Mathf.Deg2Rad)
            );

            Vector3 spawnPos = player.position + (Vector3)(dir * spawnOffset);

            BoomerangWeapon b = Instantiate(boomerangPrefab, spawnPos, Quaternion.identity);
            b.Init(player, dir);
        }
    }
}
