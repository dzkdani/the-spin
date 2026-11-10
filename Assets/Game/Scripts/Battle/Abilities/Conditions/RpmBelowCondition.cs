using System;

[Serializable]
public sealed class RpmBelowCondition : IAbilityCondition
{
    public float normalizedRpm;

    public bool Evaluate(in AbilityContext context)
    {
        return context.Self.RpmNormalized < normalizedRpm;
    }
}
