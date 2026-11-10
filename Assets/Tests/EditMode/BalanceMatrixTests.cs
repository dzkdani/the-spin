using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class BalanceMatrixTests
{
    [TearDown]
    public void TearDown()
    {
        ProductionReflection.CallStatic("ContentRegistry", "Clear");
    }

    [Test]
    public void ReferenceCatalogHasEighteenBuilds()
    {
        IEnumerable builds = LoadCatalog();

        List<string> ids = new List<string>();

        foreach (object build in builds)
            ids.Add((string)ProductionReflection.Field(build, "Id"));

        Assert.AreEqual(18, ids.Count);
        CollectionAssert.Contains(ids, "atk_crye");
        CollectionAssert.Contains(ids, "bal_pyre");
        CollectionAssert.Contains(ids, "def_sparke");
        Assert.AreEqual(ids.Count, ids.Distinct().Count());
    }

    [Test]
    public void ReferenceBuildsInstallInnatePassives()
    {
        List<object> builds = LoadCatalog().Cast<object>().ToList();

        object atk = Find(builds, "atk_pyre");
        object bal = Find(builds, "bal_pyre");
        object def = Find(builds, "def_pyre");

        Assert.AreEqual(
            "surge_strike",
            (string)ProductionReflection.Field(
                ProductionReflection.Field(atk, "Innate"),
                "id"));

        Assert.AreEqual(
            0.6f,
            GetStat(bal, "MatchupScale"),
            0.0001f);

        Assert.AreEqual(
            22f,
            GetStat(def, "Defense"),
            0.0001f);
    }

    [Test]
    public void MatrixIsDeterministicAndFlagsOutOfBandBuilds()
    {
        object first = RunMatrix();
        object second = RunMatrix();

        string csv = (string)ProductionReflection.Call(first, "ToCsv");

        Assert.AreEqual(csv, ProductionReflection.Call(second, "ToCsv"));

        string[] lines = csv
            .Split('\n')
            .Select(line => line.TrimEnd('\r'))
            .Where(line => line.Length > 0)
            .ToArray();

        Assert.AreEqual(4, lines.Length);
        Assert.AreEqual("build,strong,mid,weak", lines[0]);
        Assert.IsTrue(lines[1].Contains(",-,"));

        IEnumerable outside = (IEnumerable)ProductionReflection.Call(
            first,
            "BuildsOutsideBand",
            0.4f,
            0.6f);

        List<string> flagged = new List<string>();

        foreach (string entry in outside)
            flagged.Add(entry);

        Assert.IsTrue(flagged.Any(entry => entry.StartsWith("strong")));
        Assert.IsTrue(flagged.Any(entry => entry.StartsWith("weak")));
    }

    private static IEnumerable LoadCatalog()
    {
        object manifest = AssetDatabase.LoadAssetAtPath(
            "Assets/Game/Datas/ContentManifest.asset",
            typeof(ScriptableObject));

        Assert.IsNotNull(manifest);

        ProductionReflection.CallStatic(
            "ContentRegistry",
            "Initialize",
            manifest);

        return (IEnumerable)ProductionReflection.CallStatic(
            "BalanceCatalog",
            "CreateReferenceBuilds");
    }

    private static object Find(List<object> builds, string id)
    {
        for (int i = 0; i < builds.Count; i++)
        {
            if ((string)ProductionReflection.Field(builds[i], "Id") == id)
                return builds[i];
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

    private static object RunMatrix()
    {
        Type listType = typeof(List<>).MakeGenericType(
            ProductionReflection.TypeOf("BalanceBuild"));

        IList builds = (IList)Activator.CreateInstance(listType);

        builds.Add(Build("strong", 50f));
        builds.Add(Build("mid", 12f));
        builds.Add(Build("weak", 1f));

        object tuning = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("CombatTuning"));

        return ProductionReflection.CallStatic(
            "BalanceMatrix",
            "Run",
            builds,
            4,
            null,
            tuning);
    }

    private static object Build(string id, float attack)
    {
        object stats = ProductionReflection.New("StatBlock");

        SetBase(stats, "Attack", attack);
        SetBase(stats, "Weight", 5f);
        SetBase(stats, "Defense", 0f);
        SetBase(stats, "RpmDecay", 1f);
        SetBase(stats, "MatchupScale", 1f);

        return ProductionReflection.New(
            "BalanceBuild",
            id,
            stats,
            3000f,
            null);
    }

    private static void SetBase(object stats, string stat, float value)
    {
        ProductionReflection.Call(
            stats,
            "SetBase",
            ProductionReflection.EnumValue("StatId", stat),
            value);
    }
}
