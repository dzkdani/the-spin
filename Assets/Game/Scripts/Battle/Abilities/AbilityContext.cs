public readonly struct AbilityContext
{
    public readonly IBeyActions Self;
    public readonly IBeyView Opponent;
    public readonly ImpactEvent? Impact;
    public readonly float Now;
    public readonly string SourceId;
    public readonly float AbilityPower;

    public AbilityContext(
        IBeyActions self,
        IBeyView opponent,
        ImpactEvent? impact,
        float now,
        string sourceId,
        float abilityPower)
    {
        Self = self;
        Opponent = opponent;
        Impact = impact;
        Now = now;
        SourceId = sourceId;
        AbilityPower = abilityPower;
    }
}
