using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Part",
    menuName = "Bey/Part")]
public class PartDefinition : ContentDefinition
{
    public PartSlotDefinition slot;

    public List<StatModifierSpec> statModifiers =
        new List<StatModifierSpec>();

    public ElementDefinition elementClaim;

    public List<AbilityDefinition> grantedAbilities =
        new List<AbilityDefinition>();

    public int budgetCost;
}
