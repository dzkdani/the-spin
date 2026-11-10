using System;

[Serializable]
public sealed class DrainRpmEffect : IAbilityEffect
{
    public float normalizedAmount = 0.15f;

    public void Apply(in AbilityContext context)
    {
        context.Self.DrainRpm(
            normalizedAmount * context.Self.MaxRpm);
    }
}
