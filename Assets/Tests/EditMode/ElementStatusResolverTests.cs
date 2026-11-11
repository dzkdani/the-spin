using System.Collections;
using NUnit.Framework;
using UnityEngine;

public class ElementStatusResolverTests
{
    [Test]
    public void OpponentStatusIsAppliedAndExpires()
    {
        object element = Element("pyre", false, false, 2f);
        AddSpec(element, "Defense", "Add", 5f);

        object attacker = Stats();
        object defender = Stats();

        ProductionReflection.CallStatic(
            "ElementStatusResolver",
            "Apply",
            element,
            attacker,
            defender,
            10f);

        Assert.AreEqual(5f, Get(defender, "Defense"), 0.0001f);
        Assert.AreEqual(0f, Get(attacker, "Defense"), 0.0001f);

        ProductionReflection.Call(defender, "Tick", 12.1f);

        Assert.AreEqual(0f, Get(defender, "Defense"), 0.0001f);
    }

    [Test]
    public void SelfStatusTargetsAttacker()
    {
        object element = Element("sparke", false, true, 2f);
        AddSpec(element, "Attack", "Multiply", 1.1f);

        object attacker = Stats();
        object defender = Stats();

        ProductionReflection.CallStatic(
            "ElementStatusResolver",
            "Apply",
            element,
            attacker,
            defender,
            0f);

        Assert.AreEqual(11f, Get(attacker, "Attack"), 0.0001f);
        Assert.AreEqual(10f, Get(defender, "Attack"), 0.0001f);
    }

    private static object Element(
        string id,
        bool neutral,
        bool affectsSelf,
        float duration)
    {
        object element = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("ElementDefinition"));

        ProductionReflection.SetField(element, "id", id);
        ProductionReflection.SetField(element, "isNeutral", neutral);
        ProductionReflection.SetField(element, "onHitAffectsSelf", affectsSelf);
        ProductionReflection.SetField(element, "onHitDuration", duration);

        return element;
    }

    private static void AddSpec(
        object element,
        string stat,
        string op,
        float value)
    {
        object spec = ProductionReflection.New("StatModifierSpec");

        ProductionReflection.SetField(
            spec,
            "stat",
            ProductionReflection.EnumValue("StatId", stat));

        ProductionReflection.SetField(
            spec,
            "op",
            ProductionReflection.EnumValue("ModOp", op));

        ProductionReflection.SetField(spec, "value", value);

        IList modifiers = (IList)ProductionReflection.Field(
            element,
            "onHitModifiers");

        modifiers.Add(spec);
    }

    private static object Stats()
    {
        object stats = ProductionReflection.New("StatBlock");

        SetBase(stats, "Attack", 10f);
        SetBase(stats, "Defense", 0f);
        SetBase(stats, "Weight", 5f);

        return stats;
    }

    private static void SetBase(object stats, string stat, float value)
    {
        ProductionReflection.Call(
            stats,
            "SetBase",
            ProductionReflection.EnumValue("StatId", stat),
            value);
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
