using UnityEngine;

public class RopeScript : MonoBehaviour
{
    [SerializeField] private LineRenderer line;
    [SerializeField] private Transform handle;
    [SerializeField] private Transform rat;

    [SerializeField] private int points = 20;

    void Update()
    {
        line.positionCount = points;

        Vector3 start = handle.position;
        Vector3 end = rat.position;

        for(int i = 0; i < points; i++)
        {
            float t = i / (float)(points - 1);

            Vector3 pos = Vector3.Lerp(start, end, t);

            pos.y += Mathf.Sin(t * Mathf.PI) * -0.3f;

            line.SetPosition(i, pos);
        }
    }
}
