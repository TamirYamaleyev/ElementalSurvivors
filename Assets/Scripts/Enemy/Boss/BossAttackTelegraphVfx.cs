using UnityEngine;

public sealed class BossAttackTelegraphVfx : MonoBehaviour
{
    [SerializeField] private GameObject vfxRoot;
    [SerializeField] private float stopHideDelay = 0.15f;

    private ParticleSystem[] particleSystems;
    private float hideAt = -1f;

    private void Awake()
    {
        if (vfxRoot == null)
            vfxRoot = gameObject;

        particleSystems = vfxRoot.GetComponentsInChildren<ParticleSystem>(true);
        StopImmediate();
    }

    private void Update()
    {
        if (hideAt < 0f || Time.time < hideAt)
            return;

        hideAt = -1f;
        if (vfxRoot != null)
            vfxRoot.SetActive(false);
    }

    public void Play(Vector2 aimDirection)
    {
        if (vfxRoot == null)
            return;

        hideAt = -1f;
        vfxRoot.SetActive(true);
        ApplyDirectionRotation(aimDirection);

        if (particleSystems == null || particleSystems.Length == 0)
            particleSystems = vfxRoot.GetComponentsInChildren<ParticleSystem>(true);

        foreach (var ps in particleSystems)
        {
            ps.Clear(true);
            ps.Play(true);
        }
    }

    public void SetDirection(Vector2 direction)
    {
        if (vfxRoot == null || !vfxRoot.activeSelf)
            return;

        ApplyDirectionRotation(direction);
    }

    public void Stop()
    {
        if (vfxRoot == null)
            return;

        if (particleSystems == null)
            particleSystems = vfxRoot.GetComponentsInChildren<ParticleSystem>(true);

        foreach (var ps in particleSystems)
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        hideAt = Time.time + stopHideDelay;
    }

    public void StopImmediate()
    {
        hideAt = -1f;

        if (vfxRoot == null)
            return;

        if (particleSystems == null)
            particleSystems = vfxRoot.GetComponentsInChildren<ParticleSystem>(true);

        foreach (var ps in particleSystems)
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        vfxRoot.SetActive(false);
    }

    private void ApplyDirectionRotation(Vector2 aimDirection)
    {
        if (aimDirection.sqrMagnitude <= 1e-6f)
            return;

        var angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90f;
        vfxRoot.transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
