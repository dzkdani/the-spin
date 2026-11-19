using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class WaveRampTests
{
    private static readonly string[] Profiles =
    {
        "AI_Calm", "AI_Pressure", "AI_Tank",
        "AI_Counter", "AI_Climax", "AI_Gyre"
    };

    [Test]
    public void DashPressureNeverFallsAcrossWaves()
    {
        int previous = -1;

        for (int i = 0; i < Profiles.Length; i++)
        {
            object profile = AssetDatabase.LoadAssetAtPath(
                "Assets/Game/Datas/Enemies/" + Profiles[i] + ".asset",
                ProductionReflection.TypeOf("AiProfile"));

            Assert.IsNotNull(profile, Profiles[i] + " must exist.");

            object result = ProductionReflection.CallStatic(
                "AiSimulation", "Run", profile, 7, 60);

            int dashes = (int)ProductionReflection.Field(result, "Dashes");

            Assert.GreaterOrEqual(
                dashes,
                previous,
                Profiles[i] + " should be at least as aggressive as the previous wave.");

            previous = dashes;
        }
    }
}
