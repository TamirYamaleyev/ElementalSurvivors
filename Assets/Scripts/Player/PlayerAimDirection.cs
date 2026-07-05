using UnityEngine;

public class PlayerAimDirection : MonoBehaviour
{
    [SerializeField] private Camera mainCam;

    public Vector2 LastDirection { get; private set; } = Vector2.right;

    public Vector2 MouseScreenPosition { get; private set; }
    public Vector2 MouseWorldPosition {  get; private set; }

    public void SetMousePosition(Vector2 screenPosition)
    {
        MouseScreenPosition = screenPosition;

        Vector3 screen = screenPosition;
        screen.z = -mainCam.transform.position.z;

        MouseWorldPosition = mainCam.ScreenToWorldPoint(screen);
    }

    public void SetDirection(Vector2 input)
    {
        if (input.sqrMagnitude < 0.01f)
            return;

        LastDirection = input.normalized;
    }
}