public sealed class SpinRule : IImpactRule
{
    public int Order => 300;

    public void Apply(ImpactState state, ImpactInput input)
    {
        if (!input.OppositeSpin)
            return;

        state.PowerA *= input.Tuning.oppositeSpinMultiplier;
        state.PowerB *= input.Tuning.oppositeSpinMultiplier;
    }
}
