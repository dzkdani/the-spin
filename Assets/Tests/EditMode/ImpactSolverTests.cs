using System.Collections;
using NUnit.Framework;
using UnityEngine;

public class ImpactSolverTests
{
    [Test]
    public void ZeroSpeedDealsNoDamage()
    {
        object result = Resolve(0f, 0f);

        Assert.AreEqual(0f, ReadFloat(result, "DamageToA"), 0.0001f);
        Assert.AreEqual(0f, ReadFloat(result, "DamageToB"), 0.0001f);
        Assert.AreEqual(0f, ReadVector(result, "ImpulseOnA").magnitude, 0.0001f);
    }

    [Test]
    public void DefenseReducesIncomingDamage()
    {
        object noDefense = Resolve(16f, 0f);
        object withDefense = Resolve(16f, 20f);

        float without = ReadFloat(noDefense, "DamageToA");
        float with = ReadFloat(withDefense, "DamageToA");

        Assert.AreEqual(without * 40f / 60f, with, 0.0001f);
    }

    [Test]
    public void KnockbackIsClampedToMaxKnockback()
    {
        object result = Resolve(16f, 0f);

        Assert.LessOrEqual(
            ReadVector(result, "ImpulseOnB").magnitude,
            12f + 0.0001f);
    }

    [Test]
    public void HeavierTargetTakesLessKnockback()
    {
        object light = Resolve(16f, 0f, weightB: 1f);
        object heavy = Resolve(16f, 0f, weightB: 10f);

        Assert.Greater(
            ReadVector(light, "ImpulseOnB").magnitude,
            ReadVector(heavy, "ImpulseOnB").magnitude);
    }

    [Test]
    public void OppositeSpinScalesBothSides()
    {
        object same = Resolve(16f, 0f, oppositeSpin: false);
        object opposite = Resolve(16f, 0f, oppositeSpin: true);

        Assert.AreEqual(
            ReadFloat(same, "DamageToB") * 1.5f,
            ReadFloat(opposite, "DamageToB"),
            0.0001f);
    }

    [Test]
    public void PendingModIsAppliedAndReported()
    {
        object input = CreateInput(16f, 0f);
        IList pendingA = (IList)ProductionReflection.Field(input, "PendingA");

        pendingA.Add(CreatePending("charge", 1.75f));

        object result = ProductionReflection.CallStatic(
            "ImpactSolver",
            "Resolve",
            input);

        object plain = Resolve(16f, 0f);

        Assert.AreEqual(
            ReadFloat(plain, "DamageToB") * 1.75f,
            ReadFloat(result, "DamageToB"),
            0.0001f);

        IList used = (IList)ProductionReflection.Field(
            result,
            "PendingModsUsedA");

        Assert.AreEqual(1, used.Count);
        Assert.AreEqual("charge", used[0]);
    }

    private static object Resolve(
        float speed,
        float defenseA,
        float weightB = 5f,
        bool oppositeSpin = false)
    {
        object input = CreateInput(speed, defenseA, weightB, oppositeSpin);

        return ProductionReflection.CallStatic(
            "ImpactSolver",
            "Resolve",
            input);
    }

    private static object CreateInput(
        float speed,
        float defenseA,
        float weightB = 5f,
        bool oppositeSpin = false)
    {
        object input = ProductionReflection.New("ImpactInput");

        ProductionReflection.SetField(input, "Speed", speed);
        ProductionReflection.SetField(input, "RpmA", 1f);
        ProductionReflection.SetField(input, "RpmB", 1f);
        ProductionReflection.SetField(input, "OppositeSpin", oppositeSpin);
        ProductionReflection.SetField(input, "Normal", Vector2.right);
        ProductionReflection.SetField(input, "Now", 0f);
        ProductionReflection.SetField(input, "Tuning", CreateTuning());
        ProductionReflection.SetField(input, "StatsA", CreateStats(defenseA, 5f));
        ProductionReflection.SetField(input, "StatsB", CreateStats(0f, weightB));

        return input;
    }

    private static object CreateStats(float defense, float weight)
    {
        object stats = ProductionReflection.New("StatBlock");

        ProductionReflection.Call(
            stats,
            "SetBase",
            ProductionReflection.EnumValue("StatId", "Attack"),
            10f);

        ProductionReflection.Call(
            stats,
            "SetBase",
            ProductionReflection.EnumValue("StatId", "Defense"),
            defense);

        ProductionReflection.Call(
            stats,
            "SetBase",
            ProductionReflection.EnumValue("StatId", "Weight"),
            weight);

        return stats;
    }

    private static object CreateTuning()
    {
        return ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("CombatTuning"));
    }

    private static object CreatePending(string source, float multiplier)
    {
        return ProductionReflection.New(
            "PendingImpactMod",
            source,
            multiplier,
            1,
            float.PositiveInfinity);
    }

    private static float ReadFloat(object instance, string fieldName)
    {
        return (float)ProductionReflection.Field(instance, fieldName);
    }

    private static Vector2 ReadVector(object instance, string fieldName)
    {
        return (Vector2)ProductionReflection.Field(instance, fieldName);
    }
}
