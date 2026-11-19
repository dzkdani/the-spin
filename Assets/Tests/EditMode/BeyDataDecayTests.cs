using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class BeyDataDecayTests
{
    [Test]
    public void ApplyDecayUsesTypeMultiplier()
    {
        GameObject gameObject = new GameObject("DecayBey");
        gameObject.SetActive(false);

        Component data = gameObject.AddComponent(
            ProductionReflection.TypeOf("BeyData"));

        object stats = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("BeyStats"));

        SetField(stats, "maxRPM", 1000f);
        SetField(stats, "attack", 10f);
        SetField(stats, "weight", 5f);
        SetField(stats, "rpmDecayRate", 0.1f);

        data.GetType()
            .GetField(
                "<Base>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(data, stats);

        gameObject.SetActive(true);

        ProductionReflection.CallPrivate(data, "Awake");

        object statBlock = ProductionReflection.Property(data, "Stats");

        ProductionReflection.Call(
            statBlock,
            "SetBase",
            ProductionReflection.EnumValue("StatId", "RpmDecay"),
            2f);

        ProductionReflection.Call(data, "ApplyDecay", 1f);

        Assert.AreEqual(
            800f,
            (float)ProductionReflection.Property(data, "CurrentRPM"),
            0.01f);

        Object.DestroyImmediate(gameObject);
    }

    [Test]
    public void RestoreRpmRefillsCurrentRpm()
    {
        GameObject gameObject = new GameObject("RestoreBey");
        gameObject.SetActive(false);

        Component data = gameObject.AddComponent(
            ProductionReflection.TypeOf("BeyData"));

        object stats = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("BeyStats"));

        SetField(stats, "maxRPM", 3000f);
        SetField(stats, "attack", 10f);
        SetField(stats, "weight", 5f);

        data.GetType()
            .GetField(
                "<Base>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(data, stats);

        gameObject.SetActive(true);
        ProductionReflection.CallPrivate(data, "Awake");

        ProductionReflection.Call(data, "ReduceRPM", 1200f);

        Assert.AreEqual(
            1800f,
            (float)ProductionReflection.Property(data, "CurrentRPM"),
            0.01f);

        ProductionReflection.Call(data, "RestoreRPM");

        Assert.AreEqual(
            3000f,
            (float)ProductionReflection.Property(data, "CurrentRPM"),
            0.01f);

        Object.DestroyImmediate(gameObject);
    }

    private static void SetField(object instance, string field, object value)
    {
        instance
            .GetType()
            .GetField(field)
            .SetValue(instance, value);
    }
}
