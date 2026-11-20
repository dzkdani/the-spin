using System.Collections;
using NUnit.Framework;
using UnityEngine;

public class GarageCoreTests
{
    [Test]
    public void ClearAllRemovesInstalledAbilities()
    {
        GameObject host = new GameObject("Runner");
        host.SetActive(false);

        try
        {
            object data = host.AddComponent(
                ProductionReflection.TypeOf("BeyData"));

            object runner = host.AddComponent(
                ProductionReflection.TypeOf("AbilityRunner"));

            ProductionReflection.Call(
                runner, "Initialize", data, null, null, null);

            object ability = ScriptableObject.CreateInstance(
                ProductionReflection.TypeOf("AbilityDefinition"));

            ProductionReflection.SetField(ability, "id", "test_active");
            ProductionReflection.SetField(
                ability,
                "kind",
                ProductionReflection.EnumValue("AbilityKind", "Active"));

            Assert.IsTrue((bool)ProductionReflection.Call(
                runner, "Install", ability, "test:source"));
            Assert.AreEqual(1, AbilityCount(runner));

            ProductionReflection.Call(runner, "ClearAll");
            Assert.AreEqual(0, AbilityCount(runner));
        }
        finally
        {
            Object.DestroyImmediate(host);
        }
    }

    [Test]
    public void ConfigureAppliesBlueprintChassis()
    {
        GameObject host = new GameObject("Bey");
        host.SetActive(false);

        try
        {
            object data = host.AddComponent(
                ProductionReflection.TypeOf("BeyData"));

            object stats = ScriptableObject.CreateInstance(
                ProductionReflection.TypeOf("BeyStats"));
            ProductionReflection.SetField(stats, "maxRPM", 1000f);

            object blueprint = ScriptableObject.CreateInstance(
                ProductionReflection.TypeOf("BeyBlueprint"));
            ProductionReflection.SetField(blueprint, "chassis", stats);

            ProductionReflection.Call(data, "Configure", blueprint);

            Assert.AreEqual(
                stats,
                ProductionReflection.Property(data, "Base"));

            Assert.AreEqual(
                1000f,
                (float)ProductionReflection.Property(data, "CurrentRPM"));
        }
        finally
        {
            Object.DestroyImmediate(host);
        }
    }

    [Test]
    public void GarageModelCyclesAndValidates()
    {
        object tuning = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("CombatTuning"));

        object chassis = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("BeyStats"));
        ProductionReflection.SetField(chassis, "maxRPM", 5000f);

        object bal = MakeContent("BeyTypeDefinition", "bal");
        object atk = MakeContent("BeyTypeDefinition", "atk");
        object crye = MakeContent("ElementDefinition", "crye");

        object bladeSlot = MakeContent("PartSlotDefinition", "blade");
        ProductionReflection.SetField(bladeSlot, "elementPriority", 30);

        object bitSlot = MakeContent("PartSlotDefinition", "bit");
        ProductionReflection.SetField(bitSlot, "elementPriority", 10);

        object blade = MakeContent("PartDefinition", "pyre_blade");
        ProductionReflection.SetField(blade, "slot", bladeSlot);
        ProductionReflection.SetField(blade, "budgetCost", 3);

        object bit = MakeContent("PartDefinition", "grip_bit");
        ProductionReflection.SetField(bit, "slot", bitSlot);
        ProductionReflection.SetField(bit, "budgetCost", 3);

        object template = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("BeyBlueprint"));
        ProductionReflection.SetField(template, "chassis", chassis);
        ProductionReflection.SetField(template, "type", bal);
        ProductionReflection.SetField(template, "baseElement", crye);

        IList templateParts =
            (IList)ProductionReflection.Field(template, "parts");
        templateParts.Add(blade);
        templateParts.Add(bit);

        object model = ProductionReflection.New(
            "GarageModel",
            template,
            tuning,
            ArrayOf("BeyTypeDefinition", bal, atk),
            ArrayOf("ElementDefinition", crye),
            ArrayOf("PartDefinition", blade, bit));

        IList rows = (IList)ProductionReflection.Property(model, "Rows");
        Assert.AreEqual(4, rows.Count);
        Assert.AreEqual(6, (int)ProductionReflection.Property(model, "Budget"));
        Assert.IsTrue((bool)ProductionReflection.Property(model, "IsValid"));
        Assert.AreEqual(2, CandidateParts(model).Count);

        ProductionReflection.SetField(tuning, "partBudgetCap", 5);
        ProductionReflection.Call(model, "Cycle", 0, 1);

        Assert.AreEqual(
            atk,
            ProductionReflection.Field(Candidate(model), "type"));
        Assert.IsFalse((bool)ProductionReflection.Property(model, "IsValid"));

        ProductionReflection.SetField(tuning, "partBudgetCap", 10);
        ProductionReflection.Call(model, "Cycle", 0, 1);
        Assert.IsTrue((bool)ProductionReflection.Property(model, "IsValid"));

        ProductionReflection.Call(model, "Cycle", 2, 1);
        Assert.AreEqual(1, CandidateParts(model).Count);

        ProductionReflection.Call(model, "Cycle", 3, 1);
        Assert.AreEqual(0, CandidateParts(model).Count);
    }

    private static object MakeContent(string typeName, string id)
    {
        object value = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf(typeName));
        ProductionReflection.SetField(value, "id", id);
        return value;
    }

    private static object ArrayOf(string typeName, params object[] values)
    {
        System.Array array = System.Array.CreateInstance(
            ProductionReflection.TypeOf(typeName),
            values.Length);

        for (int i = 0; i < values.Length; i++)
            array.SetValue(values[i], i);

        return array;
    }

    private static object Candidate(object model)
    {
        return ProductionReflection.Property(model, "Candidate");
    }

    private static IList CandidateParts(object model)
    {
        return (IList)ProductionReflection.Field(Candidate(model), "parts");
    }

    private static int AbilityCount(object runner)
    {
        object value = ProductionReflection.Field(runner, "abilities");
        return ((ICollection)value).Count;
    }
}
