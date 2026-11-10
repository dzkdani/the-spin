using UnityEngine;

[CreateAssetMenu(
    fileName = "BeyType",
    menuName = "Bey/Bey Type")]
public class BeyTypeDefinition : ContentDefinition
{
    [Header("Profile")]
    public float attackMultiplier = 1f;

    public float defenseFlat;

    public float weightMultiplier = 1f;

    public float rpmDecayMultiplier = 1f;

    [Header("Innate")]
    public string innatePassiveId;
}
