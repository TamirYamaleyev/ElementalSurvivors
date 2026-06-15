using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class CollectRadiusController : MonoBehaviour
{
    [SerializeField] private PlayerPickupFacade pickupFacade;
    [SerializeField] private MonoBehaviour statsProviderBehaviour;

    private IPlayerStatsProvider _statsProvider;
    private CircleCollider2D _collider;

    void Awake()
    {
        _collider = GetComponent<CircleCollider2D>();
        if (_collider == null)
            _collider = gameObject.AddComponent<CircleCollider2D>();
        _collider.isTrigger = true;

        if (statsProviderBehaviour == null)
        {
            foreach (var mb in transform.root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb == null || mb == this)
                    continue;
                if (mb is IPlayerStatsProvider)
                {
                    statsProviderBehaviour = mb;
                    break;
                }
            }
        }

        if (statsProviderBehaviour is IPlayerStatsProvider provider)
            _statsProvider = provider;
        else if (statsProviderBehaviour != null)
            Debug.LogError($"{nameof(CollectRadiusController)}: assigned stats provider does not implement {nameof(IPlayerStatsProvider)}.", this);

        if (pickupFacade == null)
            pickupFacade = GetComponentInParent<PlayerPickupFacade>();
        if (pickupFacade == null)
            pickupFacade = transform.root.GetComponent<PlayerPickupFacade>();
        if (pickupFacade == null)
            pickupFacade = transform.root.gameObject.AddComponent<PlayerPickupFacade>();
    }

    void OnEnable()
    {
        if (_statsProvider != null)
        {
            _statsProvider.OnStatsChanged += HandleStatsChanged;
            ApplyRadius(_statsProvider.Current);
        }
    }

    void OnDisable()
    {
        if (_statsProvider != null)
            _statsProvider.OnStatsChanged -= HandleStatsChanged;
    }

    private void HandleStatsChanged(PlayerStatsSnapshot snapshot)
    {
        ApplyRadius(snapshot);
    }

    private void ApplyRadius(PlayerStatsSnapshot snapshot)
    {
        _collider.radius = snapshot.CollectRadius;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (pickupFacade == null)
            return;

        if (other.TryGetComponent<ICollectible>(out var collectible))
        {
            collectible.Collect(pickupFacade);
            Destroy(other.gameObject);
        }
    }
}
