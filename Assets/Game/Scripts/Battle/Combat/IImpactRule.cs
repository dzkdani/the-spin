public interface IImpactRule
{
    int Order { get; }

    void Apply(ImpactState state, ImpactInput input);
}
