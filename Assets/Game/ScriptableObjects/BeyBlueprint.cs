using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "BeyBlueprint",
    menuName = "Bey/Bey Blueprint")]
public class BeyBlueprint : ContentDefinition
{
    public BeyStats chassis;

    public BeyTypeDefinition type;

    public ElementDefinition baseElement;

    public AiProfile aiProfile;

    public List<PartDefinition> parts =
        new List<PartDefinition>();

    public List<AbilityDefinition> equippedPassives =
        new List<AbilityDefinition>();

    public List<AbilityDefinition> equippedActives =
        new List<AbilityDefinition>();
}
