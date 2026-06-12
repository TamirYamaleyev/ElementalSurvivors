using UnityEngine;

[CreateAssetMenu(fileName = "ReactionVfxCatalog", menuName = "Elemental Survivors/Reaction VFX Catalog")]
public class ReactionVfxCatalogSO : ScriptableObject
{
    [SerializeField] private GameObject vaporize;
    [SerializeField] private GameObject crystallize;
    [SerializeField] private GameObject scorchingWind;
    [SerializeField] private GameObject explosion;
    [SerializeField] private GameObject growth;
    [SerializeField] private GameObject hail;
    [SerializeField] private GameObject electrowetting;
    [SerializeField] private GameObject dustSandStorm;
    [SerializeField] private GameObject magnetism;
    [SerializeField] private GameObject staticCharge;

    public static void NormalizePair(StatusType a, StatusType b, out StatusType first, out StatusType second)
    {
        if (a == b)
        {
            first = second = a;
            return;
        }

        if (a < b)
        {
            first = a;
            second = b;
        }
        else
        {
            first = b;
            second = a;
        }
    }

    /// <summary>Returns null if pair is not one of the 10 elemental reaction pairs.</summary>
    public GameObject GetPrefab(StatusType a, StatusType b)
    {
        NormalizePair(a, b, out var x, out var y);
        if (x == y)
            return null;

        return (x, y) switch
        {
            (StatusType.Fire, StatusType.Water) => vaporize,
            (StatusType.Fire, StatusType.Earth) => crystallize,
            (StatusType.Fire, StatusType.Wind) => scorchingWind,
            (StatusType.Fire, StatusType.Lightning) => explosion,
            (StatusType.Water, StatusType.Wind) => hail,
            (StatusType.Water, StatusType.Earth) => growth,
            (StatusType.Water, StatusType.Lightning) => electrowetting,
            (StatusType.Wind, StatusType.Earth) => dustSandStorm,
            (StatusType.Wind, StatusType.Lightning) => magnetism,
            (StatusType.Earth, StatusType.Lightning) => staticCharge,
            _ => null
        };
    }
}
