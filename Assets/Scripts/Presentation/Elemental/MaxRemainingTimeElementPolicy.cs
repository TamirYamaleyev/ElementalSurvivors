using System.Collections.Generic;

public sealed class MaxRemainingTimeElementPolicy : IActiveElementVisualPolicy
{
    public bool TryPickPrimaryElement(IReadOnlyList<StatusSnapshot> active, out StatusType primary)
    {
        primary = default;
        if (active == null || active.Count == 0)
            return false;

        float best = -1f;
        for (int i = 0; i < active.Count; i++)
        {
            if (active[i].Remaining > best)
            {
                best = active[i].Remaining;
                primary = active[i].Type;
            }
        }

        return best >= 0f;
    }
}
