public sealed class MatchupRule : IImpactRule
{
    public int Order => 200;

    public void Apply(ImpactState state, ImpactInput input)
    {
        state.PowerA *= MatchupResolver.Get(
            input.Matchups,
            input.ElementA,
            input.ElementB);

        state.PowerB *= MatchupResolver.Get(
            input.Matchups,
            input.ElementB,
            input.ElementA);
    }
}
