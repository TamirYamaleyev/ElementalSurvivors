using UnityEngine;

public static class ReactionVfxSortingUtility
{
    public static void ApplyAboveEnemy(GameObject root, Enemy enemy, int orderOffset)
    {
        if (root == null || enemy == null)
            return;

        var body = enemy.GetComponentInChildren<SpriteRenderer>();
        if (body == null)
            return;

        var layerId = body.sortingLayerID;
        var order = body.sortingOrder + orderOffset;

        foreach (var rnd in root.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            rnd.sortingLayerID = layerId;
            rnd.sortingOrder = order;
        }
    }
}
