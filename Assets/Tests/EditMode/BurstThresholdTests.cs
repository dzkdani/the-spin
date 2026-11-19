using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class BurstThresholdTests
{
    [Test]
    public void BurstRequiresEightyPercentOfMaxRpm()
    {
        object stats = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("BeyStats"));

        ProductionReflection.SetField(stats, "maxRPM", 1000f);

        object data = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("BeyData"));

        data.GetType()
            .GetField(
                "<Base>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(data, stats);

        Assert.IsTrue((bool)ProductionReflection.CallStatic(
            "GameManager",
            "IsBurst",
            800f,
            data));

        Assert.IsFalse((bool)ProductionReflection.CallStatic(
            "GameManager",
            "IsBurst",
            799f,
            data));
    }
}
