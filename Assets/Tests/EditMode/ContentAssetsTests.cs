using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;

public class ContentAssetsTests
{
    [Test]
    public void BeyAssetsReferenceTypesAndElements()
    {
        object player = Load("Assets/Game/Datas/BasePlayer.asset");
        object enemy = Load("Assets/Game/Datas/BaseEnemy.asset");

        Assert.AreEqual("bal", Id(ProductionReflection.Field(player, "type")));
        Assert.AreEqual("crye", Id(ProductionReflection.Field(player, "baseElement")));

        Assert.AreEqual("atk", Id(ProductionReflection.Field(enemy, "type")));
        Assert.AreEqual("pyre", Id(ProductionReflection.Field(enemy, "baseElement")));
    }

    [Test]
    public void TypeProfilesMatchDesign()
    {
        object atk = Load("Assets/Game/Datas/Types/Atk.asset");
        object def = Load("Assets/Game/Datas/Types/Def.asset");
        object bal = Load("Assets/Game/Datas/Types/Bal.asset");

        Assert.AreEqual(1.25f, (float)ProductionReflection.Field(atk, "attackMultiplier"), 0.0001f);
        Assert.AreEqual("surge_strike", (string)ProductionReflection.Field(atk, "innatePassiveId"));

        Assert.AreEqual(20f, (float)ProductionReflection.Field(def, "defenseFlat"), 0.0001f);
        Assert.AreEqual(1.2f, (float)ProductionReflection.Field(def, "weightMultiplier"), 0.0001f);
        Assert.AreEqual(0.81f, (float)ProductionReflection.Field(def, "attackMultiplier"), 0.0001f);
        Assert.AreEqual("hard_shell", (string)ProductionReflection.Field(def, "innatePassiveId"));

        Assert.AreEqual(8f, (float)ProductionReflection.Field(bal, "defenseFlat"), 0.0001f);
        Assert.AreEqual(1.065f, (float)ProductionReflection.Field(bal, "attackMultiplier"), 0.0001f);
        Assert.AreEqual("steady_core", (string)ProductionReflection.Field(bal, "innatePassiveId"));
    }

    [Test]
    public void MatchupTableHasRingPairs()
    {
        object table = Load("Assets/Game/Datas/MatchupTable.asset");
        IList entries = (IList)ProductionReflection.Field(table, "entries");

        Assert.AreEqual(10, entries.Count);

        Assert.AreEqual(1.25f, FindMultiplier(entries, "pyre", "crye"), 0.0001f);
        Assert.AreEqual(0.8f, FindMultiplier(entries, "crye", "pyre"), 0.0001f);
        Assert.AreEqual(1.25f, FindMultiplier(entries, "hydre", "pyre"), 0.0001f);
    }

    [Test]
    public void ExampleAbilityKitIsAuthored()
    {
        string[] guids = AssetDatabase.FindAssets(
            "t:AbilityDefinition",
            new[] { "Assets/Game/Datas/Abilities" });

        List<string> ids = new List<string>();

        for (int i = 0; i < guids.Length; i++)
        {
            object ability = AssetDatabase.LoadAssetAtPath(
                AssetDatabase.GUIDToAssetPath(guids[i]),
                typeof(UnityEngine.ScriptableObject));

            Assert.IsNotNull(ability);
            ids.Add(Id(ability));
        }

        CollectionAssert.Contains(ids, "counter_spin");
        CollectionAssert.Contains(ids, "ember_core");
        CollectionAssert.Contains(ids, "last_spin");
        CollectionAssert.Contains(ids, "ignition");
    }

    [Test]
    public void RingElementsHaveOnHitStatuses()
    {
        object pyre = Load("Assets/Game/Datas/Pyre.asset");
        object sparke = Load("Assets/Game/Datas/Sparke.asset");

        IList pyreMods = (IList)ProductionReflection.Field(
            pyre,
            "onHitModifiers");

        Assert.AreEqual(1, pyreMods.Count);
        Assert.AreEqual(
            "RpmDecay",
            ProductionReflection.Field(pyreMods[0], "stat").ToString());
        Assert.AreEqual(
            1.8f,
            (float)ProductionReflection.Field(pyreMods[0], "value"),
            0.0001f);

        Assert.IsTrue((bool)ProductionReflection.Field(
            sparke,
            "onHitAffectsSelf"));
    }

    [Test]
    public void ManifestValidatesCleanly()
    {
        object manifest = Load("Assets/Game/Datas/ContentManifest.asset");

        IEnumerable errors = (IEnumerable)ProductionReflection.CallStatic(
            "ContentValidator",
            "Validate",
            manifest);

        int count = 0;

        foreach (object error in errors)
        {
            Assert.Fail((string)error);
            count++;
        }

        Assert.AreEqual(0, count);
    }

    private static float FindMultiplier(IList entries, string attacker, string defender)
    {
        foreach (object entry in entries)
        {
            object attackElement = ProductionReflection.Field(entry, "attacker");
            object defendElement = ProductionReflection.Field(entry, "defender");

            if (Id(attackElement) == attacker && Id(defendElement) == defender)
                return (float)ProductionReflection.Field(entry, "multiplier");
        }

        Assert.Fail($"Missing matchup {attacker} > {defender}.");

        return 0f;
    }

    private static string Id(object content)
    {
        return (string)ProductionReflection.Field(content, "id");
    }

    private static object Load(string path)
    {
        object asset = AssetDatabase.LoadAssetAtPath(
            path,
            typeof(UnityEngine.ScriptableObject));

        Assert.IsNotNull(asset, $"Missing asset: {path}");

        return asset;
    }
}
