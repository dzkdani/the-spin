using System;

[Serializable]
public sealed class OpponentDashingCondition : IAbilityCondition
{
    public bool Evaluate(in AbilityContext context)
    {
        return context.Opponent.IsDashing;
    }
}
