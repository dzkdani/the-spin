using UnityEngine;

public sealed class BalanceBuild
{
    public string Id;
    public StatBlock Stats;
    public float MaxRpm;
    public ElementDefinition Element;

    public BalanceBuild(
        string id,
        StatBlock stats,
        float maxRpm,
        ElementDefinition element)
    {
        Id = id;
        Stats = stats;
        MaxRpm = maxRpm;
        Element = element;
    }
}
