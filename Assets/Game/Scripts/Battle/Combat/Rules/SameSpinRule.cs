public sealed class SameSpinRule : IImpactRule
{
    public int Order => 350;

    public void Apply(ImpactState state, ImpactInput input)
    {
        if (input.OppositeSpin)
            return;

        state.PowerA *= input.Tuning.sameSpinMultiplier;
        state.PowerB *= input.Tuning.sameSpinMultiplier;
    }
}
