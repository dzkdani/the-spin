using System;

[Serializable]
public sealed class ClaimElementEffect : IAbilityEffect
{
    public ElementDefinition element;
    public int priority = 50;
    public float duration = 4f;

    public void Apply(in AbilityContext context)
    {
        if (element == null)
            return;

        float expiresAt = duration <= 0f
            ? float.PositiveInfinity
            : context.Now + duration;

        context.Self.ClaimElement(
            element,
            priority,
            expiresAt,
            context.SourceId);
    }
}
