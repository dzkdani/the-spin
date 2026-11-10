using System.Collections.Generic;

public static class BeyBlueprintValidator
{
    public static List<string> Validate(
        BeyBlueprint blueprint,
        CombatTuning tuning)
    {
        List<string> errors = new List<string>();

        if (blueprint == null)
        {
            errors.Add("Blueprint is missing.");
            return errors;
        }

        if (blueprint.chassis == null)
            errors.Add("Blueprint has no chassis.");

        HashSet<string> slots = new HashSet<string>();
        HashSet<string> abilityIds = new HashSet<string>();
        int budget = 0;

        int grantedPassives = 0;
        int grantedActives = 0;

        for (int i = 0; i < blueprint.parts.Count; i++)
        {
            PartDefinition part = blueprint.parts[i];

            if (part == null)
            {
                errors.Add($"Part entry {i} is null.");
                continue;
            }

            if (part.slot == null)
            {
                errors.Add($"Part '{part.id}' has no slot.");
            }
            else if (!slots.Add(part.slot.id))
            {
                errors.Add($"Multiple parts use slot '{part.slot.id}'.");
            }

            budget += part.budgetCost;

            for (int j = 0; j < part.grantedAbilities.Count; j++)
            {
                AbilityDefinition granted = part.grantedAbilities[j];

                if (granted == null)
                {
                    errors.Add($"Part '{part.id}' grants a null ability.");
                    continue;
                }

                if (!abilityIds.Add(granted.id))
                    errors.Add($"Duplicate ability id '{granted.id}'.");

                if (granted.kind == AbilityKind.Passive)
                    grantedPassives++;
                else
                    grantedActives++;
            }
        }

        if (budget > tuning.partBudgetCap)
        {
            errors.Add(
                $"Part budget {budget} exceeds cap {tuning.partBudgetCap}.");
        }

        if (grantedPassives > tuning.maxGrantedPassives)
            errors.Add("Too many granted passives.");

        if (grantedActives > tuning.maxGrantedActives)
            errors.Add("Too many granted actives.");

        int equippedPassives = blueprint.equippedPassives.Count;
        int equippedActives = blueprint.equippedActives.Count;

        if (equippedPassives > tuning.maxEquippedPassives)
            errors.Add("Too many equipped passives.");

        if (equippedActives > tuning.maxEquippedActives)
            errors.Add("Too many equipped actives.");

        for (int i = 0; i < blueprint.equippedPassives.Count; i++)
        {
            AbilityDefinition ability = blueprint.equippedPassives[i];

            if (ability == null)
            {
                errors.Add($"Equipped passive {i} is null.");
                continue;
            }

            if (!abilityIds.Add(ability.id))
                errors.Add($"Duplicate ability id '{ability.id}'.");
        }

        for (int i = 0; i < blueprint.equippedActives.Count; i++)
        {
            AbilityDefinition ability = blueprint.equippedActives[i];

            if (ability == null)
            {
                errors.Add($"Equipped active {i} is null.");
                continue;
            }

            if (!abilityIds.Add(ability.id))
                errors.Add($"Duplicate ability id '{ability.id}'.");

            if (ability.active.rpmCost > 0.5f)
                errors.Add($"Active '{ability.id}' costs more than 0.5.");
        }

        return errors;
    }
}
