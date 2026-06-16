using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDefaultWeapon : MonoBehaviour
{
    [SerializeField] private Transform spear;
    [SerializeField] private Camera cam;
    [SerializeField] private PlayerCharacterAnimation characterAnimation;

    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackRadius = 2f;
    [SerializeField] private float activeTime = 0.12f;

    [SerializeField] private float thrustInterval = 0.05f;
    [SerializeField] private int[] thrustsPerLevel = { 1, 2, 3 };
    [SerializeField] private MonoBehaviour statsProviderBehaviour;

    private IPlayerStatsProvider _statsProvider;
    private int currentLevel;
    private int ThrustCount => thrustsPerLevel[currentLevel];

    private float timer;
    private Vector2 lastDirection = Vector2.right;

    void Awake()
    {
        if (statsProviderBehaviour is IPlayerStatsProvider provider)
            _statsProvider = provider;
        else
            _statsProvider = GetComponentInParent<IPlayerStatsProvider>();

        if (characterAnimation == null)
            characterAnimation = GetComponent<PlayerCharacterAnimation>();
    }

    void Start()
    {
        currentLevel = 0;
    }

    void Update()
    {
        UpdateDirection();

        timer += Time.deltaTime;

        float effectiveCooldown = ResolveCooldown();
        if (timer >= effectiveCooldown)
        {
            timer = 0f;
            StartCoroutine(Attack());
        }
    }

    public void LevelUp()
    {
        if (currentLevel >= thrustsPerLevel.Length - 1) return;
        currentLevel++;
    }

    private float ResolveCooldown()
    {
        if (_statsProvider == null)
            return attackCooldown;

        return CombatStatResolver.ScaleCooldown(attackCooldown, _statsProvider.Current);
    }

    private void UpdateDirection()
    {
        if (Mouse.current == null) return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        Vector2 dir = mouseWorldPos - transform.position;

        if (dir.sqrMagnitude > 0.0001f)
            lastDirection = dir.normalized;
    }

    private IEnumerator Attack()
    {
        characterAnimation?.NotifyAttack();

        for (int t = 0; t < ThrustCount; t++)
        {
            Vector2 dir = lastDirection;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            spear.localPosition = dir * attackRadius;
            spear.localRotation = Quaternion.Euler(0, 0, angle);

            spear.gameObject.SetActive(true);

            yield return new WaitForSeconds(activeTime);

            spear.gameObject.SetActive(false);

            if (t < ThrustCount - 1)
                yield return new WaitForSeconds(thrustInterval);
        }
    }
}
