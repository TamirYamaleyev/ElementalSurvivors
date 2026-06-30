using System.Collections.Generic;
using UnityEngine;

public class DamageNumberDisplay : MonoBehaviour
{
    public static DamageNumberDisplay Instance { get; private set; }

    [SerializeField] private DamageNumberView prefab;
    [SerializeField] private RectTransform container;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private int prewarmCount = 16;
    [SerializeField] private Color color = new(1f, 0.95f, 0.4f, 1f);
    [SerializeField] private float lifetime = 0.8f;
    [SerializeField] private Vector2 worldOffset = new(0f, 0.5f);
    [SerializeField] private Vector2 randomSpread = new(0.35f, 0.15f);
    [SerializeField] private int canvasSortingOrder = 100;

    private readonly Stack<DamageNumberView> pool = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        if (worldCamera == null)
            worldCamera = Camera.main;

        EnsureContainerDrawOrder();
        Prewarm();
    }

    private void EnsureContainerDrawOrder()
    {
        if (container == null || canvasSortingOrder < 0)
            return;

        var canvas = container.GetComponent<Canvas>();
        if (canvas == null)
            canvas = container.GetComponentInParent<Canvas>();

        if (canvas != null)
            canvas.sortingOrder = Mathf.Max(canvas.sortingOrder, canvasSortingOrder);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void DisplayDamageNumber(float damage, Vector3 worldPosition)
    {
        DisplayDamageNumber(damage, worldPosition, null);
    }

    public void DisplayDamageNumber(float damage, Vector3 worldPosition, Color? overrideColor)
    {
        if (prefab == null || container == null || damage <= 0f)
            return;

        Vector3 spawnPosition = worldPosition + (Vector3)worldOffset;
        spawnPosition.x += Random.Range(-randomSpread.x, randomSpread.x);
        spawnPosition.y += Random.Range(-randomSpread.y, randomSpread.y);

        DamageNumberView view = Acquire();
        view.Play(Mathf.RoundToInt(damage), overrideColor ?? color, spawnPosition, lifetime, Release);
    }

    private void Prewarm()
    {
        if (prefab == null || container == null || prewarmCount <= 0)
            return;

        for (int i = 0; i < prewarmCount; i++)
        {
            DamageNumberView view = CreateInstance();
            view.gameObject.SetActive(false);
            pool.Push(view);
        }
    }

    private DamageNumberView Acquire()
    {
        return pool.Count > 0 ? pool.Pop() : CreateInstance();
    }

    private void Release(DamageNumberView view)
    {
        if (view == null)
            return;

        view.gameObject.SetActive(false);
        pool.Push(view);
    }

    private DamageNumberView CreateInstance()
    {
        DamageNumberView view = Instantiate(prefab, container);
        view.gameObject.SetActive(false);
        return view;
    }
}
