using System;

[Serializable]
public sealed class ScaleNextImpactEffect : IAbilityEffect
{
    public float multiplier = 1.75f;
    public int count = 1;
    public float duration = 10f;

    public void Apply(in AbilityContext context)
    {
        float scaledMultiplier =
            1f + (multiplier - 1f) * context.AbilityPower;

        float expiresAt = duration <= 0f
            ? float.PositiveInfinity
            : context.Now + duration;

        context.Self.AddPendingImpact(
            new PendingImpactMod(
                context.SourceId,
                scaledMultiplier,
                count,
                expiresAt));
    }
}
