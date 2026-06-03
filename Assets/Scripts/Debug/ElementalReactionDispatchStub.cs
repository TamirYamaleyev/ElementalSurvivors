using UnityEngine;

/// <summary>
/// Placeholder referenced by <c>ElementalVfxTest</c> scene; reserved for future reaction/burst routing from <see cref="StatusSystem"/>.
/// </summary>
public sealed class ElementalReactionDispatchStub : MonoBehaviour
{
    [SerializeField] private ElementalReactionCatalogData catalog;
    [SerializeField] private float minIntervalSeconds = 0.12f;

    public ElementalReactionCatalogData Catalog => catalog;
    public float MinIntervalSeconds => minIntervalSeconds;
}
