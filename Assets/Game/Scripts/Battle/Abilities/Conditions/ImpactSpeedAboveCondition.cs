using System;

[Serializable]
public sealed class ImpactSpeedAboveCondition : IAbilityCondition
{
    public float speed;

    public bool Evaluate(in AbilityContext context)
    {
        return context.Impact.HasValue &&
            context.Impact.Value.Speed > speed;
    }
}
