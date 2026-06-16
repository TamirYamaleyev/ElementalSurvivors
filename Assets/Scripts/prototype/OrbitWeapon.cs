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
    private bool initialized;

    public void LevelUp()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        EnsureInitialized();

        if (currentOrbCount == levelOneOrbCount)
            currentOrbCount = levelTwoOrbCount;
        else if (currentOrbCount == levelTwoOrbCount)
            currentOrbCount = levelThreeOrbCount;
        else if (currentOrbCount == levelThreeOrbCount)
            currentOrbCount = levelFourOrbCount;
        else if (currentOrbCount == levelFourOrbCount)
            currentOrbCount = levelFiveOrbCount;

        while (orbs.Count < currentOrbCount)
        {
            GameObject orb = Instantiate(orbPrefab, transform);
            orbs.Add(orb.transform);
        }
    }

    void Update()
    {
        if (!initialized || orbs == null || orbs.Count == 0)
            return;

        currentAngle += rotationSpeed * Time.deltaTime;

        float angleStep = 360f / currentOrbCount;

        for (int i = 0; i < orbs.Count; i++)
        {
            float angle = currentAngle + i * angleStep;
            float rad = angle * Mathf.Deg2Rad;

            Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
            orbs[i].localPosition = offset;
        }
    }

    private void EnsureInitialized()
    {
        if (initialized)
            return;

        if (orbPrefab == null)
        {
            Debug.LogError($"{nameof(OrbitWeapon)} on '{name}' is missing {nameof(orbPrefab)}.", this);
            return;
        }

        orbs = new List<Transform>();
        currentOrbCount = levelOneOrbCount;

        for (int i = 0; i < currentOrbCount; i++)
        {
            GameObject orb = Instantiate(orbPrefab, transform);
            orbs.Add(orb.transform);
        }

        initialized = true;
    }
}
