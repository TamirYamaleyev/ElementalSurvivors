using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

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

    void Start()
    {
        orbs = new List<Transform>();

        currentOrbCount = levelOneOrbCount;

        for (int i = 0; i < currentOrbCount; i++)
        {
            GameObject orb = Instantiate(orbPrefab, transform);
            orbs.Add(orb.transform);
        }
    }

    public void LevelUp()
    {
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
}
