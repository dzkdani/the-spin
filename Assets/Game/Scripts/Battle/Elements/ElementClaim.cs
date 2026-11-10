public readonly struct ElementClaim
{
    public readonly ElementDefinition Element;
    public readonly int Priority;
    public readonly string SourceId;
    public readonly float ExpiresAt;
    public readonly long Sequence;

    public ElementClaim(
        ElementDefinition element,
        int priority,
        string sourceId,
        float expiresAt,
        long sequence)
    {
        Element = element;
        Priority = priority;
        SourceId = sourceId;
        ExpiresAt = expiresAt;
        Sequence = sequence;
    }

    public bool IsExpired(float now)
    {
        return now >= ExpiresAt;
    }
}
