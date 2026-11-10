using System;

[Serializable]
public sealed class AddModifierEffect : IAbilityEffect
{
    public StatId stat;
    public ModOp op;
    public float value;
    public float duration = -1f;
    public StackPolicy stacking = StackPolicy.Refresh;
    public int maxStacks = 1;
    public string scope;

    public void Apply(in AbilityContext context)
    {
        float expiresAt = duration <= 0f
            ? float.PositiveInfinity
            : context.Now + duration;

        context.Self.Stats.Add(
            new StatModifier(
                stat,
                op,
                value,
                context.SourceId,
                scope,
                expiresAt,
                stacking,
                maxStacks),
            context.Now);
    }
}
