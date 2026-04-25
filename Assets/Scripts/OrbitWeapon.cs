using UnityEngine;

public class OrbitWeapon : MonoBehaviour
{
    [SerializeField] private GameObject orbPrefab;
    [SerializeField] private int orbCount = 3;
    [SerializeField] private float radius = 2f;
    [SerializeField] private float rotationSpeed = 180f;

    private Transform[] orbs;
    private float currentAngle;

    void Start()
    {
        orbs = new Transform[orbCount];

        for (int i = 0; i < orbCount; i++)
        {
            GameObject orb = Instantiate(orbPrefab, transform);
            orbs[i] = orb.transform;
        }
    }

    void Update()
    {
        currentAngle += rotationSpeed * Time.deltaTime;

        float angleStep = 360f / orbCount;

        for (int i = 0; i < orbCount; i++)
        {
            float angle = currentAngle + i * angleStep;
            float rad = angle * Mathf.Deg2Rad;

            Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;

            orbs[i].localPosition = offset;
        }
    }
}
