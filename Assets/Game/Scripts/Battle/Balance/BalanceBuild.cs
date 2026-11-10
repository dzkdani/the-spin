using UnityEngine;

public sealed class BalanceBuild
{
    public string Id;
    public StatBlock Stats;
    public float MaxRpm;
    public ElementDefinition Element;
    public AbilityDefinition Innate;

    public BalanceBuild(
        string id,
        StatBlock stats,
        float maxRpm,
        ElementDefinition element)
        : this(id, stats, maxRpm, element, null)
    {
    }

    public BalanceBuild(
        string id,
        StatBlock stats,
        float maxRpm,
        ElementDefinition element,
        AbilityDefinition innate)
    {
        Id = id;
        Stats = stats;
        MaxRpm = maxRpm;
        Element = element;
        Innate = innate;
    }
}
