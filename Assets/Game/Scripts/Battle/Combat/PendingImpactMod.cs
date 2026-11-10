public readonly struct PendingImpactMod
{
    public readonly string SourceId;
    public readonly float Multiplier;
    public readonly int Remaining;
    public readonly float ExpiresAt;

    public PendingImpactMod(
        string sourceId,
        float multiplier,
        int remaining,
        float expiresAt)
    {
        SourceId = sourceId;
        Multiplier = multiplier;
        Remaining = remaining;
        ExpiresAt = expiresAt;
    }
}
