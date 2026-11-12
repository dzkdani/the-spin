using System.Collections.Generic;
using UnityEngine;

public sealed class BalanceBuild
{
    public string Id;
    public StatBlock Stats;
    public float MaxRpm;
    public float RpmDecayRate = 0.0015f;
    public ElementDefinition Element;
    public AbilityDefinition Innate;
    public List<AbilityDefinition> Actives = new List<AbilityDefinition>();

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
