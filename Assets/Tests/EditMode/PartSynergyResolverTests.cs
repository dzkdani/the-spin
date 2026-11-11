using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PartSynergyResolverTests
{
    [Test]
    public void TwoMatchingElementsGrantAttackAndDefense()
    {
        object pyre = Element("pyre", false);
        IList parts = Parts(
            Part("pyre_a", pyre),
            Part("pyre_b", pyre));

        object stats = Stats();

        ProductionReflection.CallStatic(
            "PartSynergyResolver",
            "Apply",
            parts,
            stats,
            0f);

        Assert.AreEqual(10.5f, Get(stats, "Attack"), 0.0001f);
        Assert.AreEqual(10.5f, Get(stats, "Defense"), 0.0001f);
    }

    [Test]
    public void MixedElementsDoNotSynergize()
    {
        IList parts = Parts(
            Part("pyre", Element("pyre", false)),
            Part("crye", Element("crye", false)));

        object stats = Stats();

        ProductionReflection.CallStatic(
            "PartSynergyResolver",
            "Apply",
            parts,
            stats,
            0f);

        Assert.AreEqual(10f, Get(stats, "Attack"), 0.0001f);
        Assert.AreEqual(10f, Get(stats, "Defense"), 0.0001f);
    }

    [Test]
    public void NeutralPartsDoNotSynergize()
    {
        object neutral = Element("neutral", true);
        IList parts = Parts(
            Part("neutral_a", neutral),
            Part("neutral_b", neutral));

        object stats = Stats();

        ProductionReflection.CallStatic(
            "PartSynergyResolver",
            "Apply",
            parts,
            stats,
            0f);

        Assert.AreEqual(10f, Get(stats, "Attack"), 0.0001f);
    }

    private static IList Parts(params object[] parts)
    {
        Type listType = typeof(List<>).MakeGenericType(
            ProductionReflection.TypeOf("PartDefinition"));

        IList list = (IList)Activator.CreateInstance(listType);

        for (int i = 0; i < parts.Length; i++)
            list.Add(parts[i]);

        return list;
    }

    private static object Part(string id, object element)
    {
        object part = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("PartDefinition"));

        ProductionReflection.SetField(part, "id", id);
        ProductionReflection.SetField(part, "elementClaim", element);

        return part;
    }

    private static object Element(string id, bool neutral)
    {
        object element = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("ElementDefinition"));

        ProductionReflection.SetField(element, "id", id);
        ProductionReflection.SetField(element, "isNeutral", neutral);

        return element;
    }

    private static object Stats()
    {
        object stats = ProductionReflection.New("StatBlock");

        SetBase(stats, "Attack", 10f);
        SetBase(stats, "Defense", 10f);

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
