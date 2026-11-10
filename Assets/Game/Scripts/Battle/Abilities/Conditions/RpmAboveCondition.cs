using System;

[Serializable]
public sealed class RpmAboveCondition : IAbilityCondition
{
    public float normalizedRpm;

    public bool Evaluate(in AbilityContext context)
    {
        return context.Self.RpmNormalized > normalizedRpm;
    }
}
