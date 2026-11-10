public readonly struct StatContribution
{
    public readonly string SourceId;
    public readonly ModOp Op;
    public readonly float Value;
    public readonly string Scope;
    public readonly float ExpiresAt;

    public StatContribution(
        string sourceId,
        ModOp op,
        float value,
        string scope,
        float expiresAt)
    {
        SourceId = sourceId;
        Op = op;
        Value = value;
        Scope = scope;
        ExpiresAt = expiresAt;
    }
}
