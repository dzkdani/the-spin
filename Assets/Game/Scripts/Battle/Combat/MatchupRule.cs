public sealed class MatchupRule : IImpactRule
{
    public int Order => 200;

    public void Apply(ImpactState state, ImpactInput input)
    {
        state.PowerA *= Scale(
            MatchupResolver.Get(
                input.Matchups,
                input.ElementA,
                input.ElementB),
            input.StatsA.Get(StatId.MatchupScale));

        state.PowerB *= Scale(
            MatchupResolver.Get(
                input.Matchups,
                input.ElementB,
                input.ElementA),
            input.StatsB.Get(StatId.MatchupScale));
    }

    private static float Scale(float multiplier, float scale)
    {
        return 1f + (multiplier - 1f) * scale;
    }
}
