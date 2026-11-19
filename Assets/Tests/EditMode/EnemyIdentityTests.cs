using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class EnemyIdentityTests
{
    private static readonly string[] Waves =
    {
        "Wave1Neutral", "Wave2Pyre", "Wave3Hydre",
        "Wave4Crye", "Wave5Sparke", "Wave6Gyre"
    };

    [Test]
    public void EveryNonTutorialWaveHasAnActive()
    {
        for (int i = 0; i < Waves.Length; i++)
        {
            object blueprint = Load(Waves[i], "BeyBlueprint");

            if (i == 0)
                Assert.IsFalse(HasActive(blueprint), "Neutral stays a tutorial wave.");
            else
                Assert.IsTrue(HasActive(blueprint), Waves[i] + " needs an active.");
        }
    }

    [Test]
    public void NoBlueprintDuplicatesAnAbilityId()
    {
        for (int i = 0; i < Waves.Length; i++)
        {
            List<string> ids = AbilityIds(Load(Waves[i], "BeyBlueprint"));
            HashSet<string> unique = new HashSet<string>(ids);

            Assert.AreEqual(
                ids.Count,
                unique.Count,
                Waves[i] + " installs an ability twice.");
        }
    }

    [Test]
    public void DefensiveProfilesBaitMoreThanAggressiveProfiles()
    {
        float tank = ReadWeight("AI_Tank", "baitWeight");
        float pressure = ReadWeight("AI_Pressure", "baitWeight");

        Assert.Greater(tank, pressure);
    }

    [Test]
    public void ActiveBehaviorFiresAboveOldOpponentThreshold()
    {
        object profile = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("AiProfile"));

        ProductionReflection.SetField(profile, "activeWeight", 1f);

        object snapshot = ProductionReflection.New(
            "BattleSnapshot",
            0.5f,
            0.6f,
            Vector2.zero,
            new Vector2(2f, 0f),
            2f,
            null,
            null,
            false,
            1f,
            0f,
            new[] { true },
            new[] { 0f });

        object rule = ProductionReflection.New("UseActiveBehavior");

        Assert.Greater(
            (float)ProductionReflection.Call(rule, "Score", snapshot, profile),
            0f);
    }

    private static object Load(string name, string type)
    {
        object asset = AssetDatabase.LoadAssetAtPath(
            "Assets/Game/Datas/Enemies/" + name + ".asset",
            ProductionReflection.TypeOf(type));

        Assert.IsNotNull(asset, name + " must exist.");

        return asset;
    }

    private static bool HasActive(object blueprint)
    {
        IList equipped = (IList)ProductionReflection.Field(
            blueprint, "equippedActives");

        if (equipped != null && equipped.Count > 0)
            return true;

        IList parts = (IList)ProductionReflection.Field(blueprint, "parts");

        foreach (object part in parts)
        {
            if (part == null)
                continue;

            IList granted = (IList)ProductionReflection.Field(
                part, "grantedAbilities");

            if (granted == null)
                continue;

            foreach (object ability in granted)
            {
                int kind = Convert.ToInt32(
                    ProductionReflection.Field(ability, "kind"));

                if (kind == 1)
                    return true;
            }
        }

        return false;
    }

    private static List<string> AbilityIds(object blueprint)
    {
        List<string> ids = new List<string>();

        AddIds(ids, ProductionReflection.Field(blueprint, "equippedActives"));
        AddIds(ids, ProductionReflection.Field(blueprint, "equippedPassives"));

        IList parts = (IList)ProductionReflection.Field(blueprint, "parts");

        foreach (object part in parts)
        {
            if (part != null)
                AddIds(ids, ProductionReflection.Field(part, "grantedAbilities"));
        }

        return ids;
    }

    private static void AddIds(List<string> ids, object list)
    {
        if (list == null)
            return;

        foreach (object ability in (IList)list)
        {
            if (ability != null)
                ids.Add((string)ProductionReflection.Field(ability, "id"));
        }
    }

    private static float ReadWeight(string name, string field)
    {
        return (float)ProductionReflection.Field(
            Load(name, "AiProfile"), field);
    }
}
