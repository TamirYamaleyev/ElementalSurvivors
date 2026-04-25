using UnityEngine;
using UnityEngine.InputSystem;

public class BoomerangController : MonoBehaviour
{
    [SerializeField] private Camera mainCam;
    [SerializeField] private BoomerangWeapon boomerangPrefab;
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private float spawnOffset = 1f;

    private float timer;
    private Transform player;

    private Vector2 lastDirection = Vector2.right;

    void Start()
    {
        player = PlayerController.Instance;
        timer = 0f;
    }

    void Update()
    {
        UpdateDirection();

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            Fire();
            timer = cooldown;
        }
    }

    private void UpdateDirection()
    {
        if (Mouse.current == null) return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        Vector2 dir = (mouseWorldPos - (Vector3)player.position);

        if (dir.sqrMagnitude > 0.0001f)
            lastDirection = dir.normalized;
    }

    private void Fire()
    {
        Vector3 spawnPos = player.position + (Vector3)(lastDirection * spawnOffset);

        BoomerangWeapon b = Instantiate(boomerangPrefab, spawnPos, Quaternion.identity);
        b.Init(player, lastDirection);
    }
}