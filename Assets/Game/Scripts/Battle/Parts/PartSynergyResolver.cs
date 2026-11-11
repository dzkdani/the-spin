using System.Collections.Generic;

public static class PartSynergyResolver
{
    public const float Bonus = 0.05f;

    public static void Apply(
        IReadOnlyList<PartDefinition> parts,
        StatBlock stats,
        float now)
    {
        if (parts == null || stats == null)
            return;

        ElementDefinition match = FindPair(parts);

        if (match == null)
            return;

        string source = "synergy:" + match.id;

        stats.Add(
            new StatModifier(
                StatId.Attack,
                ModOp.PercentAdd,
                Bonus,
                source,
                null,
                float.PositiveInfinity,
                StackPolicy.Refresh,
                1),
            now);

        stats.Add(
            new StatModifier(
                StatId.Defense,
                ModOp.PercentAdd,
                Bonus,
                source,
                null,
                float.PositiveInfinity,
                StackPolicy.Refresh,
                1),
            now);
    }

    public static void Remove(StatBlock stats)
    {
        if (stats != null)
            stats.RemoveBySourcePrefix("synergy:");
    }

    private static ElementDefinition FindPair(
        IReadOnlyList<PartDefinition> parts)
    {
        for (int i = 0; i < parts.Count; i++)
        {
            ElementDefinition first = ResolveElement(parts[i]);

            if (first == null || first.isNeutral)
                continue;

            for (int j = i + 1; j < parts.Count; j++)
            {
                ElementDefinition second = ResolveElement(parts[j]);

                if (second != null && second.id == first.id)
                    return first;
            }
        }

        return null;
    }

    private static ElementDefinition ResolveElement(PartDefinition part)
    {
        if (part == null)
            return null;

        if (part.elementClaim != null)
            return part.elementClaim;

        for (int i = 0; i < part.grantedAbilities.Count; i++)
        {
            AbilityDefinition ability = part.grantedAbilities[i];

            if (ability == null)
                continue;

            for (int e = 0; e < ability.effects.Count; e++)
            {
                if (ability.effects[e] is ClaimElementEffect claim &&
                    claim.element != null &&
                    claim.duration <= 0f)
                {
                    return claim.element;
                }
            }
        }

        return null;
    }
}
