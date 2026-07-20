using System.Collections.Generic;
using UnityEngine;

public class EXPOrb : MonoBehaviour, ICollectible
{
    [Header("Vacuum Settings")]
    [SerializeField] private float maxSpeed = 25f;
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float closeRangeMultiplier = 3f;
    [SerializeField] private float collectDistance = 0.15f;

    private Transform target;
    private PlayerPickupFacade pickupFacade;
    private float currentSpeed;

    [SerializeField] AudioClip sfx;

    [Header("Exp Settings")]
    [SerializeField] private SpriteRenderer sr;

    [System.Serializable]
    private class ExpSpriteEntry
    {
        public float expAmount;
        public Sprite sprite;
    }

    [SerializeField] private ExpSpriteEntry[] expSprites;

    public float expToGive;

    private SortedDictionary<float, Sprite> spriteDictionary;

    void Awake()
    {
        spriteDictionary = new SortedDictionary<float, Sprite>();

        foreach (var entry in expSprites)
            spriteDictionary.Add(entry.expAmount, entry.sprite);

        UpdateSprite();
    }

    public void SetExpAmount(float amount)
    {
        expToGive = amount;
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        Sprite selectedSprite = null;

        foreach (var entry in spriteDictionary)
        {
            if (expToGive >= entry.Key)
                selectedSprite = entry.Value;
            else
                break;
        }

        if (selectedSprite != null)
            sr.sprite = selectedSprite;
    }

    public void StartVacuum(Transform player, PlayerPickupFacade pickupFacade)
    {
        target = player;
        this.pickupFacade = pickupFacade;
    }

    void Update()
    {
        if (target == null)
            return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= collectDistance)
        {
            Collect(pickupFacade);
            Destroy(gameObject);
            return;
        }

        Vector3 direction = (target.position - transform.position).normalized;

        currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);

        float speedMultiplier = Mathf.Lerp(1f, closeRangeMultiplier, 1f - Mathf.Clamp01(distance / 3f));

        transform.position += direction * currentSpeed * speedMultiplier * Time.deltaTime;
    }

    public void Collect(PlayerPickupFacade facade)
    {
        if (facade == null)
            return;

        AudioManager.Instance.PlaySfx(sfx);

        facade.AddExp(expToGive);
    }
}
