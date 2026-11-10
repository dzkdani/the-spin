public sealed class SpinStealRule : IImpactRule
{
    public int Order => 360;

    public void Apply(ImpactState state, ImpactInput input)
    {
        if (!input.OppositeSpin)
            return;

        state.StealToA = state.PowerB * input.Tuning.spinStealRate;
        state.StealToB = state.PowerA * input.Tuning.spinStealRate;
    }
}
