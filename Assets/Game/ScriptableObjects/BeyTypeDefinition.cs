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

    public void ApplyTo(StatBlock stats, float now)
    {
        string source = "type:" + id;

        stats.Add(
            new StatModifier(
                StatId.Attack,
                ModOp.Multiply,
                attackMultiplier,
                source,
                null,
                float.PositiveInfinity,
                StackPolicy.Refresh,
                1),
            now);

        stats.Add(
            new StatModifier(
                StatId.Defense,
                ModOp.Add,
                defenseFlat,
                source,
                null,
                float.PositiveInfinity,
                StackPolicy.Refresh,
                1),
            now);

        stats.Add(
            new StatModifier(
                StatId.Weight,
                ModOp.Multiply,
                weightMultiplier,
                source,
                null,
                float.PositiveInfinity,
                StackPolicy.Refresh,
                1),
            now);

        stats.Add(
            new StatModifier(
                StatId.RpmDecay,
                ModOp.Multiply,
                rpmDecayMultiplier,
                source,
                null,
                float.PositiveInfinity,
                StackPolicy.Refresh,
                1),
            now);
    }
}
