using System.Collections.Generic;

public sealed class ImpactState
{
    public float PowerA;
    public float PowerB;
    public float StealToA;
    public float StealToB;

    public readonly List<string> PendingUsedA = new List<string>();
    public readonly List<string> PendingUsedB = new List<string>();
}
