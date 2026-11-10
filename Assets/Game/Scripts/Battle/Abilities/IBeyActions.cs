public interface IBeyActions
{
    StatBlock Stats { get; }

    ElementStack Elements { get; }

    float RpmNormalized { get; }

    float MaxRpm { get; }

    void AddPendingImpact(PendingImpactMod mod);

    void DrainRpm(float amount);

    void RequestForcedDash(float power);

    void ClaimElement(
        ElementDefinition element,
        int priority,
        float expiresAt,
        string sourceId);

    string OwnerId { get; }
}
