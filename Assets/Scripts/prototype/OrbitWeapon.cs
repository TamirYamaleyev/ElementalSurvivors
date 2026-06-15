using System.Collections.Generic;
using UnityEngine;

public class OrbitWeapon : MonoBehaviour
{
    [SerializeField] private GameObject orbPrefab;
    [SerializeField] private int levelOneOrbCount = 2;
    [SerializeField] private int levelTwoOrbCount = 3;
    [SerializeField] private int levelThreeOrbCount = 4;
    [SerializeField] private int levelFourOrbCount = 5;
    [SerializeField] private int levelFiveOrbCount = 8;
    [SerializeField] private int currentOrbCount = 2;
    [SerializeField] private float radius = 2f;
    [SerializeField] private float rotationSpeed = 180f;

    private List<Transform> orbs;
    private float currentAngle;

    void Awake()
    {
        orbs = new List<Transform>();
    }

    void Start()
    {
        currentOrbCount = levelOneOrbCount;

        if (orbPrefab == null)
        {
            Debug.LogError($"{nameof(OrbitWeapon)}: assign orbPrefab on '{name}'.", this);
            return;
        }

        for (var i = 0; i < currentOrbCount; i++)
            SpawnOrb();
    }

    public void LevelUp()
    {
        if (orbs == null)
            orbs = new List<Transform>();

        if (orbPrefab == null)
        {
            Debug.LogWarning($"{nameof(OrbitWeapon)}: cannot level up — orbPrefab is not assigned on '{name}'.", this);
            return;
        }

        if (currentOrbCount == levelOneOrbCount)
            currentOrbCount = levelTwoOrbCount;
        else if (currentOrbCount == levelTwoOrbCount)
            currentOrbCount = levelThreeOrbCount;
        else if (currentOrbCount == levelThreeOrbCount)
            currentOrbCount = levelFourOrbCount;
        else if (currentOrbCount == levelFourOrbCount)
            currentOrbCount = levelFiveOrbCount;

        while (orbs.Count < currentOrbCount)
            SpawnOrb();
    }

    void Update()
    {
        if (orbs == null || orbs.Count == 0 || currentOrbCount <= 0)
            return;

        currentAngle += rotationSpeed * Time.deltaTime;

        var angleStep = 360f / currentOrbCount;

        for (var i = 0; i < orbs.Count; i++)
        {
            if (orbs[i] == null)
                continue;

            var angle = currentAngle + i * angleStep;
            var rad = angle * Mathf.Deg2Rad;

            var offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;

            orbs[i].localPosition = offset;
        }
    }

    private void SpawnOrb()
    {
        if (orbPrefab == null || orbs == null)
            return;

        var orb = Instantiate(orbPrefab, transform);
        orbs.Add(orb.transform);
    }
}
