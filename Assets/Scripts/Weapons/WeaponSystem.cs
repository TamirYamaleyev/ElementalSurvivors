using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    [SerializeField] private List<WeaponInstance> weapons;

    [SerializeField] private WeaponSystemContext context;

    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Transform orbitCenter;

    void Start()
    {
        Initialize();    
    }

    public void Initialize()
    {
        context.ProjectileSpawnPoint = projectileSpawnPoint;
        context.OrbitCenter = orbitCenter;
    }

    void Update()
    {
        foreach (var w in weapons)
        {
            var target = context.Targeting.GetNearest(transform.position, w.Current.range);
            w.Tick(Time.deltaTime, target, context);
        }
    }
}
