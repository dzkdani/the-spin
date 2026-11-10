using System;

[Serializable]
public sealed class SameSpinCondition : IAbilityCondition
{
    public bool Evaluate(in AbilityContext context)
    {
        return context.Impact.HasValue &&
            !context.Impact.Value.OppositeSpin;
    }
}
