using System;

[Serializable]
public sealed class ForceDashEffect : IAbilityEffect
{
    public float power = 1.5f;

    public void Apply(in AbilityContext context)
    {
        context.Self.RequestForcedDash(
            power * context.AbilityPower);
    }
}
