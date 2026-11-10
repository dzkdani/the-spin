public interface IBeyView
{
    ElementDefinition Element { get; }

    float RpmNormalized { get; }

    bool IsDashing { get; }
}
