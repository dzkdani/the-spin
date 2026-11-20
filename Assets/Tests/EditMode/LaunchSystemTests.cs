using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class LaunchSystemTests
{
    [Test]
    public void RotationPowerClampsToUnitRange()
    {
        Assert.AreEqual(
            0.5f,
            (float)ProductionReflection.CallStatic(
                "LaunchMath", "RotationPower", 180f, 360f),
            0.0001f);

        Assert.AreEqual(
            0f,
            (float)ProductionReflection.CallStatic(
                "LaunchMath", "RotationPower", 0f, 360f),
            0.0001f);

        Assert.AreEqual(
            1f,
            (float)ProductionReflection.CallStatic(
                "LaunchMath", "RotationPower", 360f, 360f),
            0.0001f);

        Assert.AreEqual(
            1f,
            (float)ProductionReflection.CallStatic(
                "LaunchMath", "RotationPower", -360f, 360f),
            0.0001f);

        Assert.AreEqual(
            1f,
            (float)ProductionReflection.CallStatic(
                "LaunchMath", "RotationPower", 720f, 360f),
            0.0001f);
    }

    [Test]
    public void StartRpmUsesFloor()
    {
        Assert.AreEqual(
            700f,
            (float)ProductionReflection.CallStatic(
                "LaunchMath", "StartRpm", 1000f, 0f, 0.7f),
            0.0001f);

        Assert.AreEqual(
            1000f,
            (float)ProductionReflection.CallStatic(
                "LaunchMath", "StartRpm", 1000f, 1f, 0.7f),
            0.0001f);

        Assert.AreEqual(
            850f,
            (float)ProductionReflection.CallStatic(
                "LaunchMath", "StartRpm", 1000f, 0.5f, 0.7f),
            0.0001f);
    }

    [Test]
    public void EnemyPowerHonoursZeroVariance()
    {
        Assert.AreEqual(
            0.9f,
            (float)ProductionReflection.CallStatic(
                "LaunchMath", "EnemyPower", 0.9f, 0f),
            0.0001f);
    }

    [Test]
    public void TuningCarriesEnemyLaunchSpreadAndRamp()
    {
        object tuning = AssetDatabase.LoadAssetAtPath(
            "Assets/Game/Datas/LaunchTuning.asset",
            ProductionReflection.TypeOf("LaunchTuning"));

        Assert.IsNotNull(tuning);
        Assert.AreEqual(
            0.2f,
            (float)ProductionReflection.Field(tuning, "enemyVariance"),
            0.0001f);
        Assert.Greater(
            (float)ProductionReflection.Field(tuning, "enemyPowerRamp"),
            0f);
    }

    [Test]
    public void BeyDataLaunchUsesElementFloor()
    {
        GameObject bey = new GameObject("LaunchBey");
        bey.SetActive(false);

        try
        {
            Component data = bey.AddComponent(
                ProductionReflection.TypeOf("BeyData"));

            object stats = ScriptableObject.CreateInstance(
                ProductionReflection.TypeOf("BeyStats"));

            ProductionReflection.SetField(stats, "maxRPM", 1000f);
            ProductionReflection.SetField(stats, "attack", 10f);
            ProductionReflection.SetField(stats, "weight", 5f);

            ProductionReflection.SetField(
                data, "<Base>k__BackingField", stats);

            ProductionReflection.CallPrivate(data, "Awake");
            ProductionReflection.Call(data, "Launch", 0.5f, 0.7f);

            Assert.AreEqual(
                850f,
                (float)ProductionReflection.Property(data, "CurrentRPM"),
                0.01f);
        }
        finally
        {
            Object.DestroyImmediate(bey);
        }
    }
}
