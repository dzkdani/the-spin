public static class ElementStatusResolver
{
    public static void Apply(
        ElementDefinition element,
        StatBlock attacker,
        StatBlock defender,
        float now)
    {
        if (element == null ||
            element.onHitModifiers == null ||
            element.onHitModifiers.Count == 0)
        {
            return;
        }

        StatBlock target = element.onHitAffectsSelf
            ? attacker
            : defender;

        if (target == null)
            return;

        string source = "element:" + element.id;

        float expiresAt = element.onHitDuration <= 0f
            ? float.PositiveInfinity
            : now + element.onHitDuration;

        for (int i = 0; i < element.onHitModifiers.Count; i++)
        {
            StatModifierSpec spec = element.onHitModifiers[i];

            target.Add(
                new StatModifier(
                    spec.stat,
                    spec.op,
                    spec.value,
                    source,
                    spec.scope,
                    expiresAt,
                    StackPolicy.Refresh,
                    1),
                now);
        }
    }
}
