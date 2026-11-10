using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class PartSimulationTests
{
    [TearDown]
    public void TearDown()
    {
        ProductionReflection.CallStatic("ContentRegistry", "Clear");
    }

    [Test]
    public void PartBuildAppliesStatsAndElement()
    {
        object control = Control();
        object part = Load("Assets/Game/Datas/Parts/PyreBlade.asset");

        object build = ProductionReflection.CallStatic(
            "PartSimulation",
            "CreateBuild",
            control,
            part);

        Assert.AreEqual(11.715f, GetStat(build, "Attack"), 0.001f);
        Assert.AreEqual("pyre", Id(ProductionReflection.Field(build, "Element")));
    }

    [Test]
    public void GrantedAuraClaimsItsElement()
    {
        object control = Control();
        object part = Load("Assets/Game/Datas/Parts/EmberBit.asset");

        object build = ProductionReflection.CallStatic(
            "PartSimulation",
            "CreateBuild",
            control,
            part);

        Assert.AreEqual("pyre", Id(ProductionReflection.Field(build, "Element")));
    }

    [Test]
    public void BurstBitGrantsActive()
    {
        object control = Control();
        object part = Load("Assets/Game/Datas/Parts/BurstBit.asset");

        object build = ProductionReflection.CallStatic(
            "PartSimulation",
            "CreateBuild",
            control,
            part);

        IList actives = (IList)ProductionReflection.Field(build, "Actives");

        Assert.AreEqual(1, actives.Count);
        Assert.AreEqual("burst_dash", Id(actives[0]));
    }

    [Test]
    public void ActiveUseSpendsRpmAndStartsADash()
    {
        object control = Control();
        object part = Load("Assets/Game/Datas/Parts/BurstBit.asset");

        object build = ProductionReflection.CallStatic(
            "PartSimulation",
            "CreateBuild",
            control,
            part);

        float maxRpm = (float)ProductionReflection.Field(build, "MaxRpm");

        object use = ProductionReflection.CallStatic(
            "BalanceActives",
            "TryUse",
            build,
            0,
            maxRpm,
            0f,
            0f);

        Assert.IsTrue((bool)ProductionReflection.Field(use, "Used"));
        Assert.IsTrue((bool)ProductionReflection.Field(use, "Dashing"));
        Assert.AreEqual(
            maxRpm * 0.9f,
            (float)ProductionReflection.Field(use, "RpmAfter"),
            0.1f);

        IList pending = (IList)ProductionReflection.Field(use, "Pending");

        Assert.AreEqual(2, pending.Count);
    }

    [Test]
    public void PartSimulationIsDeterministicAndCoversEveryPart()
    {
        object control = Control();
        object manifest = Load("Assets/Game/Datas/ContentManifest.asset");
        object tuning = ProductionReflection.Field(manifest, "combatTuning");
        object matchups = ProductionReflection.Field(manifest, "matchupTable");

        IList parts = LoadParts();
        IList builds = Builds();

        object first = ProductionReflection.CallStatic(
            "PartSimulation",
            "Run",
            control,
            parts,
            builds,
            2,
            matchups,
            tuning);

        object second = ProductionReflection.CallStatic(
            "PartSimulation",
            "Run",
            control,
            parts,
            builds,
            2,
            matchups,
            tuning);

        Assert.AreEqual(
            ProductionReflection.Call(first, "ToCsv"),
            ProductionReflection.Call(second, "ToCsv"));

        IList rows = (IList)ProductionReflection.Property(first, "Rows");

        Assert.AreEqual(parts.Count, rows.Count);
        Assert.AreEqual(7, rows.Count);
    }

    private static object Control()
    {
        return Find(Builds(), "bal_neutral");
    }

    private static IList Builds()
    {
        object manifest = Load("Assets/Game/Datas/ContentManifest.asset");

        ProductionReflection.CallStatic(
            "ContentRegistry",
            "Initialize",
            manifest);

        return (IList)ProductionReflection.CallStatic(
            "BalanceCatalog",
            "CreateReferenceBuilds");
    }

    private static IList LoadParts()
    {
        string[] guids = AssetDatabase.FindAssets(
            "t:PartDefinition",
            new[] { "Assets/Game/Datas/Parts" });

        Type listType = typeof(List<>).MakeGenericType(
            ProductionReflection.TypeOf("PartDefinition"));

        IList parts = (IList)Activator.CreateInstance(listType);

        List<object> loaded = new List<object>();

        for (int i = 0; i < guids.Length; i++)
        {
            loaded.Add(AssetDatabase.LoadAssetAtPath(
                AssetDatabase.GUIDToAssetPath(guids[i]),
                typeof(ScriptableObject)));
        }

        foreach (object part in loaded.OrderBy(
            item => (string)ProductionReflection.Field(item, "id")))
        {
            parts.Add(part);
        }

        return parts;
    }

    private static object Find(IList builds, string id)
    {
        foreach (object build in builds)
        {
            if ((string)ProductionReflection.Field(build, "Id") == id)
                return build;
        }

        Assert.Fail($"Missing reference build: {id}");

        return null;
    }

    private static float GetStat(object build, string stat)
    {
        object stats = ProductionReflection.Field(build, "Stats");

        return (float)ProductionReflection.Call(
            stats,
            "Get",
            ProductionReflection.EnumValue("StatId", stat),
            null);
    }

    private static string Id(object content)
    {
        return (string)ProductionReflection.Field(content, "id");
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
