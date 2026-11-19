using NUnit.Framework;
using UnityEngine;

public class BurstThresholdTests
{
    [Test]
    public void BurstRequiresEightyPercentOfMaxRpm()
    {
        GameObject target = new GameObject("BurstTarget");
        target.SetActive(false);

        try
        {
            object stats = ScriptableObject.CreateInstance(
                ProductionReflection.TypeOf("BeyStats"));

            ProductionReflection.SetField(stats, "maxRPM", 1000f);

            Component data = target.AddComponent(
                ProductionReflection.TypeOf("BeyData"));

            ProductionReflection.SetField(
                data, "<Base>k__BackingField", stats);

            Assert.IsTrue((bool)ProductionReflection.CallStatic(
                "GameManager",
                "IsBurst",
                801f,
                data));

            Assert.IsFalse((bool)ProductionReflection.CallStatic(
                "GameManager",
                "IsBurst",
                799f,
                data));
        }
        finally
        {
            Object.DestroyImmediate(target);
        }
    }
}
