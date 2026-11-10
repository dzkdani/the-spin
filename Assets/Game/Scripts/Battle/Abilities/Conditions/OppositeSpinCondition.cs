using System;

[Serializable]
public sealed class OppositeSpinCondition : IAbilityCondition
{
    public bool Evaluate(in AbilityContext context)
    {
        return context.Impact.HasValue &&
            context.Impact.Value.OppositeSpin;
    }
}
