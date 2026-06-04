using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    [SerializeField] private List<WeaponInstance> weapons;

    private WeaponSystemContext context;

    public void Initialize(WeaponSystemContext ctx)
    {
        context = ctx;
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
