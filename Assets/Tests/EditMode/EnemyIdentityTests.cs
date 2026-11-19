using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class EnemyIdentityTests
{
    [Test]
    public void EveryNonTutorialWaveHasAnActive()
    {
        string[] waves =
        {
            "Wave1Neutral", "Wave2Pyre", "Wave3Hydre",
            "Wave4Crye", "Wave5Sparke", "Wave6Gyre"
        };

        for (int i = 0; i < waves.Length; i++)
        {
            object blueprint = AssetDatabase.LoadAssetAtPath(
                "Assets/Game/Datas/Enemies/" + waves[i] + ".asset",
                ProductionReflection.TypeOf("BeyBlueprint"));

            Assert.IsNotNull(blueprint, waves[i] + " must exist.");

            System.Collections.IList actives =
                (System.Collections.IList)ProductionReflection.Field(
                    blueprint, "equippedActives");

            if (i == 0)
                Assert.AreEqual(0, actives.Count);
            else
                Assert.GreaterOrEqual(actives.Count, 1, waves[i] + " needs an active.");
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

    private static float ReadWeight(string name, string field)
    {
        object profile = AssetDatabase.LoadAssetAtPath(
            "Assets/Game/Datas/Enemies/" + name + ".asset",
            ProductionReflection.TypeOf("AiProfile"));

        return (float)ProductionReflection.Field(profile, field);
    }
}
