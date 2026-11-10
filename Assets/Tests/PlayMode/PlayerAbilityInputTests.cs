using NUnit.Framework;
using UnityEngine;

public class PlayerAbilityInputTests
{
    [Test]
    public void ConsumeAbilityRequestIsEdgeTriggered()
    {
        GameObject gameObject = new GameObject("AbilityInput");

        try
        {
            Component input = gameObject.AddComponent(
                ProductionReflection.TypeOf("PlayerBeyInput"));

            ProductionReflection.SetField(input, "abilitySlotRequest", 1);

            Assert.AreEqual(
                1,
                (int)ProductionReflection.Call(input, "ConsumeAbilityRequest"));

            Assert.AreEqual(
                -1,
                (int)ProductionReflection.Call(input, "ConsumeAbilityRequest"));
        }
        finally
        {
            Object.DestroyImmediate(gameObject);
        }
    }
}
