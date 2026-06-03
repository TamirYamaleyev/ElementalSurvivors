using UnityEngine;

/// <summary>
/// Used by <c>ElementalVfxTest</c> scene: applies one <see cref="StatusType"/> per spawned enemy so elemental DoT VFX can be compared side-by-side.
/// </summary>
public sealed class ElementalVfxTestHarness : MonoBehaviour
{
    [SerializeField] private StatusSystem statusSystem;
    [SerializeField] private float statusDuration = 999f;
    [SerializeField] private Enemy[] enemiesInElementOrder;

    private void Start()
    {
        if (statusSystem == null || enemiesInElementOrder == null)
            return;

        int n = Mathf.Min(enemiesInElementOrder.Length, (int)StatusType.Lightning + 1);
        for (int i = 0; i < n; i++)
        {
            Enemy enemy = enemiesInElementOrder[i];
            if (enemy == null)
                continue;

            statusSystem.Apply(enemy, (StatusType)i, statusDuration);
        }
    }
}
