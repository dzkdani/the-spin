using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class BeyBlueprintAssetTests
{
    [TearDown]
    public void TearDown()
    {
        ProductionReflection.CallStatic("ContentRegistry", "Clear");
    }

    [Test]
    public void PlayerAndEnemyBlueprintsValidate()
    {
        object manifest = Load("Assets/Game/Datas/ContentManifest.asset");

        ProductionReflection.CallStatic(
            "ContentRegistry",
            "Initialize",
            manifest);

        AssertBlueprintValid("Assets/Game/Datas/PlayerBlueprint.asset");
        AssertBlueprintValid("Assets/Game/Datas/EnemyBlueprint.asset");
    }

    [Test]
    public void BlueprintResolvesChassisAndElement()
    {
        object chassis = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("BeyStats"));

        ProductionReflection.SetField(chassis, "maxRPM", 1000f);

        object element = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("ElementDefinition"));

        ProductionReflection.SetField(element, "id", "pyre");

        object blueprint = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("BeyBlueprint"));

        ProductionReflection.SetField(blueprint, "chassis", chassis);
        ProductionReflection.SetField(blueprint, "baseElement", element);

        GameObject gameObject = new GameObject("BlueprintBey");
        gameObject.SetActive(false);

        Component data = gameObject.AddComponent(
            ProductionReflection.TypeOf("BeyData"));

        data.GetType()
            .GetField("blueprint", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(data, blueprint);

        gameObject.SetActive(true);
        ProductionReflection.CallPrivate(data, "Awake");

        Assert.AreSame(chassis, ProductionReflection.Property(data, "Base"));

        object elements = ProductionReflection.Property(data, "Elements");
        object current = ProductionReflection.Property(elements, "Current");

        Assert.AreEqual("pyre", (string)ProductionReflection.Field(current, "id"));

        Object.DestroyImmediate(gameObject);
    }

    [Test]
    public void PlayerBlueprintAssemblesIntoLiveBey()
    {
        object manifest = Load("Assets/Game/Datas/ContentManifest.asset");

        ProductionReflection.CallStatic(
            "ContentRegistry",
            "Initialize",
            manifest);

        object blueprint = Load("Assets/Game/Datas/PlayerBlueprint.asset");

        GameObject gameObject = new GameObject("BlueprintPlayer");
        gameObject.SetActive(false);

        Component data = gameObject.AddComponent(
            ProductionReflection.TypeOf("BeyData"));

        data.GetType()
            .GetField("blueprint", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(data, blueprint);

        gameObject.SetActive(true);
        ProductionReflection.CallPrivate(data, "Awake");

        gameObject.AddComponent(ProductionReflection.TypeOf("BeyPhysics"));
        gameObject.AddComponent(ProductionReflection.TypeOf("BeyCombat"));
        gameObject.AddComponent(ProductionReflection.TypeOf("PlayerBeyInput"));

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(gameObject.transform);
        visual.AddComponent(ProductionReflection.TypeOf("BeyVisual"));

        Component controller = gameObject.AddComponent(
            ProductionReflection.TypeOf("BeyController"));

        ProductionReflection.CallPrivate(controller, "Awake");

        object controllerData = ProductionReflection.Property(
            controller,
            "Data");

        Assert.IsNotNull(controllerData);
        Assert.IsNotNull(ProductionReflection.Property(
            controllerData,
            "Blueprint"));

        IList parts = (IList)ProductionReflection.Field(
            blueprint,
            "parts");

        Assert.AreEqual(2, parts.Count);

        ProductionReflection.CallPrivate(controller, "Start");

        Assert.AreEqual(23f, Stat(data, "Defense"), 0.0001f);
        Assert.AreEqual(1.05f, Stat(data, "TipFriction"), 0.0001f);
        Assert.AreEqual("crye", Element(data));

        Component runner = gameObject.GetComponent(
            ProductionReflection.TypeOf("AbilityRunner"));

        IList abilities = (IList)ProductionReflection.Field(
            runner,
            "abilities");

        Assert.AreEqual(3, abilities.Count);

        Object.DestroyImmediate(gameObject);
    }

    private static float Stat(object data, string stat)
    {
        return (float)ProductionReflection.Call(
            ProductionReflection.Property(data, "Stats"),
            "Get",
            ProductionReflection.EnumValue("StatId", stat),
            null);
    }

    private static string Element(object data)
    {
        object elements = ProductionReflection.Property(data, "Elements");
        object current = ProductionReflection.Property(elements, "Current");

        return (string)ProductionReflection.Field(current, "id");
    }

    private static void AssertBlueprintValid(string path)
    {
        object blueprint = Load(path);

        IEnumerable errors = (IEnumerable)ProductionReflection.CallStatic(
            "BeyBlueprintValidator",
            "Validate",
            blueprint,
            ProductionReflection.StaticProperty("ContentRegistry", "Tuning"));

        int count = 0;

        foreach (object error in errors)
        {
            Assert.Fail($"{path}: {error}");
            count++;
        }

        Assert.AreEqual(0, count);
    }

    private static object Load(string path)
    {
        object asset = AssetDatabase.LoadAssetAtPath(
            path,
            typeof(ScriptableObject));

        Assert.IsNotNull(asset, $"Missing asset: {path}");

        return asset;
    }
}
