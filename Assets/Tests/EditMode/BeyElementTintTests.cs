using NUnit.Framework;
using UnityEngine;

public class BeyElementTintTests
{
    [Test]
    public void TintUsesElementColorAndLighterShades()
    {
        GameObject bey = new GameObject("TintedBey");
        bey.SetActive(false);

        try
        {
            Component tint = bey.AddComponent(
                ProductionReflection.TypeOf("BeyElementTint"));

            SpriteRenderer outer = Layer(bey, "Outer");
            SpriteRenderer middle = Layer(bey, "Middle");
            SpriteRenderer inner = Layer(bey, "Inner");

            ProductionReflection.SetField(tint, "outerLayer", outer);
            ProductionReflection.SetField(tint, "middleLayer", middle);
            ProductionReflection.SetField(tint, "innerLayer", inner);

            object element = ScriptableObject.CreateInstance(
                ProductionReflection.TypeOf("ElementDefinition"));

            Color baseColor = new Color(0.9f, 0.2f, 0.1f, 1f);

            ProductionReflection.SetField(element, "color", baseColor);
            ProductionReflection.Call(tint, "Apply", element);

            Assert.AreEqual(baseColor, outer.color);
            Assert.AreEqual(
                Color.Lerp(baseColor, Color.white, 0.35f),
                middle.color);
            Assert.AreEqual(
                Color.Lerp(baseColor, Color.white, 0.7f),
                inner.color);
        }
        finally
        {
            Object.DestroyImmediate(bey);
        }
    }

    private static SpriteRenderer Layer(GameObject parent, string name)
    {
        GameObject layer = new GameObject(name);
        layer.transform.SetParent(parent.transform, false);

        return layer.AddComponent<SpriteRenderer>();
    }
}
