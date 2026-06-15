using UnityEngine;

/// <summary>
/// Ensures world EXP/health pickup collection works without manual scene wiring:
/// adds <see cref="PlayerPickupFacade"/> and a child trigger with <see cref="CollectRadiusController"/> when missing.
/// </summary>
public static class PlayerPickupRuntimeSetup
{
    const string ZoneObjectName = "PickupCollectZone";

    /// <summary>Idempotent; safe to call every frame (cheap early-out).</summary>
    public static void Ensure(Transform playerRoot)
    {
        if (playerRoot == null)
            return;

        if (playerRoot.GetComponentInChildren<CollectRadiusController>(true) != null)
            return;

        if (playerRoot.GetComponent<PlayerPickupFacade>() == null)
            playerRoot.gameObject.AddComponent<PlayerPickupFacade>();

        var zone = new GameObject(ZoneObjectName);
        zone.transform.SetParent(playerRoot, false);
        zone.transform.localPosition = Vector3.zero;
        zone.layer = playerRoot.gameObject.layer;
        zone.AddComponent<CollectRadiusController>();
    }
}
