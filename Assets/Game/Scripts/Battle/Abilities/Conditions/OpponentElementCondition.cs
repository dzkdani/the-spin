using System;

[Serializable]
public sealed class OpponentElementCondition : IAbilityCondition
{
    public ElementDefinition element;

    public bool Evaluate(in AbilityContext context)
    {
        ElementDefinition current = context.Opponent.Element;

        return current != null &&
            element != null &&
            current.id == element.id;
    }
}
