using System.Collections.Generic;
using UnityEngine;

public static class BeyBuilder
{
    public static bool Assemble(
        BeyBlueprint blueprint,
        BeyData data,
        AbilityRunner runner)
    {
        List<string> errors = BeyBlueprintValidator.Validate(
            blueprint,
            ContentRegistry.Tuning);

        if (errors.Count > 0)
        {
            for (int i = 0; i < errors.Count; i++)
                Debug.LogError("Blueprint: " + errors[i]);

            return false;
        }

        for (int i = 0; i < blueprint.parts.Count; i++)
            ApplyPart(blueprint.parts[i], data, runner);

        for (int i = 0; i < blueprint.equippedPassives.Count; i++)
        {
            AbilityDefinition ability = blueprint.equippedPassives[i];
            runner.Install(ability, "equipped:" + ability.id);
        }

        for (int i = 0; i < blueprint.equippedActives.Count; i++)
        {
            AbilityDefinition ability = blueprint.equippedActives[i];
            runner.Install(ability, "equipped:" + ability.id);
        }

        return true;
    }

    public static void RemovePart(
        PartDefinition part,
        BeyData data,
        AbilityRunner runner)
    {
        if (part == null)
            return;

        string sourceId = "part:" + part.id;

        data.Stats.RemoveBySource(sourceId);
        data.Elements.RemoveBySource(sourceId);

        for (int i = 0; i < part.grantedAbilities.Count; i++)
        {
            AbilityDefinition ability = part.grantedAbilities[i];

            if (ability == null)
                continue;

            runner.RemoveSource($"grant:{part.id}/{ability.id}");
        }
    }

    private static void ApplyPart(
        PartDefinition part,
        BeyData data,
        AbilityRunner runner)
    {
        string sourceId = "part:" + part.id;

        for (int i = 0; i < part.statModifiers.Count; i++)
        {
            StatModifierSpec spec = part.statModifiers[i];

            data.Stats.Add(
                new StatModifier(
                    spec.stat,
                    spec.op,
                    spec.value,
                    sourceId,
                    spec.scope,
                    float.PositiveInfinity,
                    StackPolicy.Refresh,
                    1),
                Time.time);
        }

        if (part.elementClaim != null && part.slot != null)
        {
            data.Elements.Add(
                part.elementClaim,
                part.slot.elementPriority,
                sourceId,
                float.PositiveInfinity);
        }

        for (int i = 0; i < part.grantedAbilities.Count; i++)
        {
            AbilityDefinition ability = part.grantedAbilities[i];

            if (ability == null)
                continue;

            runner.Install(
                ability,
                $"grant:{part.id}/{ability.id}");
        }
    }
}
