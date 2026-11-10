using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class ContentFoundationTests
{
    [SetUp]
    public void SetUp()
    {
        InvokeStatic("ContentRegistry", "Clear");
    }

    [TearDown]
    public void TearDown()
    {
        InvokeStatic("ContentRegistry", "Clear");
    }

    [Test]
    public void CombatTuningDefaultsMatchBaseline()
    {
        object tuning = Create("CombatTuning");

        Assert.AreEqual(0.5f, ReadFloat(tuning, "minRpmPower"));
        Assert.AreEqual(1f, ReadFloat(tuning, "maxRpmPower"));
        Assert.AreEqual(2f, ReadFloat(tuning, "damageMultiplier"));
        Assert.AreEqual(1.5f, ReadFloat(tuning, "oppositeSpinMultiplier"));
        Assert.AreEqual(1.75f, ReadFloat(tuning, "chargeMultiplier"));
        Assert.AreEqual(0.02f, ReadFloat(tuning, "shakePerSpeed"));
        Assert.AreEqual(0.05f, ReadFloat(tuning, "shakeMin"));
        Assert.AreEqual(0.35f, ReadFloat(tuning, "shakeMax"));
        Assert.AreEqual(0.001f, ReadFloat(tuning, "grindRate"));
        Assert.AreEqual(0.5f, ReadFloat(tuning, "minWeightFactor"));
        Assert.AreEqual(1.5f, ReadFloat(tuning, "maxWeightFactor"));
    }

    [Test]
    public void AiProfileDefaultsMatchBaseline()
    {
        object profile = Create("AiProfile");

        Assert.AreEqual(1.2f, ReadFloat(profile, "jitterRadius"));
        Assert.AreEqual(1f, ReadFloat(profile, "jitterInterval"));
        Assert.AreEqual(4f, ReadFloat(profile, "farDistance"));
        Assert.AreEqual(0.7f, ReadFloat(profile, "dashChanceFar"));
        Assert.AreEqual(0.2f, ReadFloat(profile, "dashChanceNear"));
        Assert.AreEqual(1.5f, ReadFloat(profile, "dashIntervalMin"));
        Assert.AreEqual(3f, ReadFloat(profile, "dashIntervalMax"));
    }

    [Test]
    public void ContentRegistryResolvesContentById()
    {
        object manifest = Create("ContentManifest");
        object neutral = Create("ElementDefinition");

        SetField(neutral, "id", "neutral");
        SetField(neutral, "isNeutral", true);

        IList assets = (IList)manifest
            .GetType()
            .GetField("assets")
            .GetValue(manifest);

        assets.Add(neutral);

        InvokeStatic("ContentRegistry", "Initialize", manifest);

        object resolved = InvokeGenericStatic(
            "ContentRegistry",
            "Get",
            ProductionType("ElementDefinition"),
            "neutral");

        Assert.AreSame(neutral, resolved);
    }

    [Test]
    public void ContentRegistryExposesDefaultTuning()
    {
        InvokeStatic("ContentRegistry", "Clear");

        object tuning = ProductionType("ContentRegistry")
            .GetProperty("Tuning", BindingFlags.Public | BindingFlags.Static)
            .GetValue(null);

        Assert.IsNotNull(tuning, "ContentRegistry.Tuning must never be null.");
        Assert.AreEqual("CombatTuning", tuning.GetType().Name);
        Assert.AreEqual(0.5f, ReadFloat(tuning, "minRpmPower"));
    }

    private static object Create(string typeName)
    {
        return ScriptableObject.CreateInstance(ProductionType(typeName));
    }

    private static float ReadFloat(object instance, string fieldName)
    {
        return (float)instance
            .GetType()
            .GetField(fieldName)
            .GetValue(instance);
    }

    private static void SetField(object instance, string fieldName, object value)
    {
        instance
            .GetType()
            .GetField(fieldName)
            .SetValue(instance, value);
    }

    private static void InvokeStatic(string typeName, string methodName, params object[] args)
    {
        MethodInfo method = ProductionType(typeName).GetMethod(methodName);

        Assert.IsNotNull(method, $"{typeName}.{methodName} must exist.");

        method.Invoke(null, args);
    }

    private static object InvokeGenericStatic(
        string typeName,
        string methodName,
        Type typeArgument,
        params object[] args)
    {
        MethodInfo method = ProductionType(typeName)
            .GetMethod(methodName)
            .MakeGenericMethod(typeArgument);

        return method.Invoke(null, args);
    }

    private static Type ProductionType(string typeName)
    {
        Type type = Type.GetType(typeName + ", Assembly-CSharp");

        Assert.IsNotNull(type, $"{typeName} must exist in Assembly-CSharp.");

        return type;
    }
}
