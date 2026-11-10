public sealed class ClashRule : IImpactRule
{
    public int Order => 450;

    public void Apply(ImpactState state, ImpactInput input)
    {
        if (!input.DashingA || !input.DashingB)
            return;

        state.PowerA *= input.Tuning.clashMultiplier;
        state.PowerB *= input.Tuning.clashMultiplier;
    }
}
