using UnityEngine;

/// <summary>
/// Destroys the reaction VFX root after a delay so world instances do not linger.
/// </summary>
public class ReactionBurstLifetime : MonoBehaviour
{
    [SerializeField] private float destroyAfterSeconds = 2.5f;

    private bool autoDestroy = true;

    public void SetDestroyAfter(float seconds)
    {
        destroyAfterSeconds = Mathf.Max(0.05f, seconds);
    }

    public void DisableAutoDestroy()
    {
        autoDestroy = false;
    }

    private void Start()
    {
        if (!autoDestroy)
            return;

        Destroy(gameObject, destroyAfterSeconds);
    }
}
