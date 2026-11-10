using NUnit.Framework;
using UnityEngine;

public class BeyTypeDefinitionTests
{
    [Test]
    public void IdentityTypeLeavesStatsUnchanged()
    {
        object stats = CreateStats();
        object type = CreateType("neutral", 1f, 0f, 1f, 1f);

        ProductionReflection.Call(type, "ApplyTo", stats, 0f);

        Assert.AreEqual(10f, Get(stats, "Attack"), 0.0001f);
        Assert.AreEqual(0f, Get(stats, "Defense"), 0.0001f);
        Assert.AreEqual(5f, Get(stats, "Weight"), 0.0001f);
        Assert.AreEqual(1f, Get(stats, "RpmDecay"), 0.0001f);
    }

    [Test]
    public void TypeModifiersApplyWithSourceTag()
    {
        object stats = CreateStats();
        object type = CreateType("atk", 1.25f, 10f, 0.9f, 1.15f);

        ProductionReflection.Call(type, "ApplyTo", stats, 0f);

        Assert.AreEqual(12.5f, Get(stats, "Attack"), 0.0001f);
        Assert.AreEqual(10f, Get(stats, "Defense"), 0.0001f);
        Assert.AreEqual(4.5f, Get(stats, "Weight"), 0.0001f);
        Assert.AreEqual(1.15f, Get(stats, "RpmDecay"), 0.0001f);

        object contributions = ProductionReflection.Call(
            stats,
            "Explain",
            ProductionReflection.EnumValue("StatId", "Attack"),
            null);

        string firstSource = null;

        foreach (object contribution in (System.Collections.IEnumerable)contributions)
        {
            firstSource = (string)ProductionReflection.Field(contribution, "SourceId");
            break;
        }

        Assert.AreEqual("type:atk", firstSource);
    }

    private static object CreateStats()
    {
        object stats = ProductionReflection.New("StatBlock");

        ProductionReflection.Call(
            stats,
            "SetBase",
            ProductionReflection.EnumValue("StatId", "Attack"),
            10f);

        ProductionReflection.Call(
            stats,
            "SetBase",
            ProductionReflection.EnumValue("StatId", "Weight"),
            5f);

        ProductionReflection.Call(
            stats,
            "SetBase",
            ProductionReflection.EnumValue("StatId", "RpmDecay"),
            1f);

        return stats;
    }

    private static object CreateType(
        string id,
        float attackMultiplier,
        float defenseFlat,
        float weightMultiplier,
        float rpmDecayMultiplier)
    {
        object type = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("BeyTypeDefinition"));

        ProductionReflection.SetField(type, "id", id);
        ProductionReflection.SetField(type, "attackMultiplier", attackMultiplier);
        ProductionReflection.SetField(type, "defenseFlat", defenseFlat);
        ProductionReflection.SetField(type, "weightMultiplier", weightMultiplier);
        ProductionReflection.SetField(type, "rpmDecayMultiplier", rpmDecayMultiplier);

        return type;
    }

    private static float Get(object stats, string stat)
    {
        return (float)ProductionReflection.Call(
            stats,
            "Get",
            ProductionReflection.EnumValue("StatId", stat),
            null);
    }
}
