public sealed class DashHitRule : IImpactRule
{
    public int Order => 500;

    public void Apply(ImpactState state, ImpactInput input)
    {
        if (input.DashingA)
            state.PowerA *= input.Tuning.dashHitMultiplier;

        if (input.DashingB)
            state.PowerB *= input.Tuning.dashHitMultiplier;
    }
}
