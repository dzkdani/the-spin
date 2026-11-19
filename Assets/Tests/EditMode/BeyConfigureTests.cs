using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class BeyConfigureTests
{
    [Test]
    public void ConfigureRebuildsIdentityFromBlueprint()
    {
        object firstChassis = Chassis(1000f);
        object firstType = Type("first", 1.2f);
        object firstElement = Element("pyre");
        object first = Blueprint(firstChassis, firstType, firstElement);

        GameObject gameObject = new GameObject("ConfigureBey");
        gameObject.SetActive(false);

        Component data = gameObject.AddComponent(
            ProductionReflection.TypeOf("BeyData"));

        SetPrivate(data, "blueprint", first);
        gameObject.SetActive(true);
        ProductionReflection.CallPrivate(data, "Awake");

        Assert.AreEqual(
            12f,
            (float)ProductionReflection.Call(
                ProductionReflection.Property(data, "Stats"),
                "Get",
                ProductionReflection.EnumValue("StatId", "Attack"),
                null),
            0.0001f);

        object secondChassis = Chassis(600f);
        object secondType = Type("second", 1f);
        object secondElement = Element("crye");
        object second = Blueprint(secondChassis, secondType, secondElement);

        ProductionReflection.Call(data, "Configure", second);

        Assert.AreSame(
            secondChassis,
            ProductionReflection.Property(data, "Base"));

        object elements = ProductionReflection.Property(data, "Elements");
        object current = ProductionReflection.Property(elements, "Current");

        Assert.AreEqual(
            "crye",
            (string)ProductionReflection.Field(current, "id"));

        Assert.AreEqual(
            600f,
            (float)ProductionReflection.Property(data, "CurrentRPM"),
            0.0001f);

        Assert.AreEqual(
            10f,
            (float)ProductionReflection.Call(
                ProductionReflection.Property(data, "Stats"),
                "Get",
                ProductionReflection.EnumValue("StatId", "Attack"),
                null),
            0.0001f);

        Object.DestroyImmediate(gameObject);
    }

    private static object Chassis(float maxRpm)
    {
        object stats = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("BeyStats"));

        ProductionReflection.SetField(stats, "maxRPM", maxRpm);
        ProductionReflection.SetField(stats, "attack", 10f);
        ProductionReflection.SetField(stats, "weight", 5f);

        return stats;
    }

    private static object Type(string id, float multiplier)
    {
        object type = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("BeyTypeDefinition"));

        ProductionReflection.SetField(type, "id", id);
        ProductionReflection.SetField(
            type,
            "attackMultiplier",
            multiplier);

        return type;
    }

    private static object Element(string id)
    {
        object element = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("ElementDefinition"));

        ProductionReflection.SetField(element, "id", id);

        return element;
    }

    private static object Blueprint(
        object chassis,
        object type,
        object element)
    {
        object blueprint = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("BeyBlueprint"));

        ProductionReflection.SetField(blueprint, "chassis", chassis);
        ProductionReflection.SetField(blueprint, "type", type);
        ProductionReflection.SetField(blueprint, "baseElement", element);

        return blueprint;
    }

    private static void SetPrivate(object target, string field, object value)
    {
        target
            .GetType()
            .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(target, value);
    }
}
