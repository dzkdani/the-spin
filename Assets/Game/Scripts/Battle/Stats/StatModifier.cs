public readonly struct StatModifier
{
    public readonly StatId Stat;
    public readonly ModOp Op;
    public readonly float Value;
    public readonly string SourceId;
    public readonly string Scope;
    public readonly float ExpiresAt;
    public readonly StackPolicy Stacking;
    public readonly int MaxStacks;

    public StatModifier(
        StatId stat,
        ModOp op,
        float value,
        string sourceId,
        string scope,
        float expiresAt,
        StackPolicy stacking,
        int maxStacks)
    {
        Stat = stat;
        Op = op;
        Value = value;
        SourceId = sourceId;
        Scope = scope;
        ExpiresAt = expiresAt;
        Stacking = stacking;
        MaxStacks = maxStacks;
    }

    public bool IsExpired(float now)
    {
        return now >= ExpiresAt;
    }

    public StatModifier WithExpiry(float expiresAt)
    {
        return new StatModifier(
            Stat,
            Op,
            Value,
            SourceId,
            Scope,
            expiresAt,
            Stacking,
            MaxStacks);
    }
}
