using System.Collections.Generic;

public interface IActiveElementVisualPolicy
{
    bool TryPickPrimaryElement(IReadOnlyList<StatusSnapshot> active, out StatusType primary);
}
