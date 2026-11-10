using System;

[Serializable]
public sealed class OwnElementCondition : IAbilityCondition
{
    public ElementDefinition element;

    public bool Evaluate(in AbilityContext context)
    {
        ElementDefinition current = context.Self.Elements.Current;

        return current != null &&
            element != null &&
            current.id == element.id;
    }
}
