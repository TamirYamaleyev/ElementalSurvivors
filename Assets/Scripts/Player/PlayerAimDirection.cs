using UnityEngine;

public class PlayerAimDirection : MonoBehaviour
{
    public Vector2 LastDirection { get; private set; } = Vector2.right;

    public void SetDirection(Vector2 input)
    {
        if (input.sqrMagnitude < 0.01f)
            return;

        LastDirection = input.normalized;
    }
}