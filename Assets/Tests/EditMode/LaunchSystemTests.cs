using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

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
    public void OverlayFillFollowsClockwiseFlag()
    {
        GameObject host = new GameObject("LaunchOverlay");
        host.SetActive(false);

        try
        {
            Component overlay = host.AddComponent(
                ProductionReflection.TypeOf("LaunchOverlay"));

            Image fill = host.AddComponent<Image>();

            ProductionReflection.SetField(overlay, "powerFill", fill);

            ProductionReflection.Call(overlay, "SetPower", 0.6f, false);

            Assert.IsFalse(fill.fillClockwise);
            Assert.AreEqual(0.6f, fill.fillAmount, 0.0001f);

            ProductionReflection.Call(overlay, "SetPower", 0.3f, true);

            Assert.IsTrue(fill.fillClockwise);
            Assert.AreEqual(0.3f, fill.fillAmount, 0.0001f);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(host);
        }
    }

    [Test]
    public void DisabledLaunchFallsBackToCallback()
    {
        GameObject host = new GameObject("DisabledLaunch");
        host.SetActive(false);

        try
        {
            Component controller = host.AddComponent(
                ProductionReflection.TypeOf("LaunchController"));

            object tuning = ScriptableObject.CreateInstance(
                ProductionReflection.TypeOf("LaunchTuning"));

            ProductionReflection.SetField(tuning, "enabled", false);
            ProductionReflection.SetField(controller, "tuning", tuning);

            bool called = false;
            Action callback = () => called = true;

            ProductionReflection.Call(
                controller, "Begin", null, null, callback);

            Assert.IsTrue(called);
            Assert.IsFalse(
                (bool)ProductionReflection.Property(controller, "IsLaunching"));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(host);
        }
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
            UnityEngine.Object.DestroyImmediate(bey);
        }
    }
}
