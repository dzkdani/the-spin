using NUnit.Framework;
using UnityEngine;

public class BalanceSimulatorTests
{
    [Test]
    public void StrongerBuildWinsMoreOften()
    {
        object strong = Build("strong", 20f);
        object weak = Build("weak", 10f);
        object tuning = CreateTuning();

        object result = ProductionReflection.CallStatic(
            "BalanceSimulator",
            "Run",
            strong,
            weak,
            50,
            null,
            tuning);

        int winsStrong = Read(result, "WinsA");
        int winsWeak = Read(result, "WinsB");
        int draws = Read(result, "Draws");

        Assert.AreEqual(50, winsStrong + winsWeak + draws);
        Assert.Greater(winsStrong, winsWeak);
    }

    [Test]
    public void SmallEdgeDoesNotAlwaysDecide()
    {
        object strong = Build("strong", 10.2f);
        object weak = Build("weak", 10f);
        object tuning = CreateTuning();

        object result = ProductionReflection.CallStatic(
            "BalanceSimulator",
            "Run",
            strong,
            weak,
            100,
            null,
            tuning);

        int winsStrong = Read(result, "WinsA");
        int winsWeak = Read(result, "WinsB");

        Assert.Greater(winsStrong, winsWeak);
        Assert.Greater(winsWeak, 0);
    }

    [Test]
    public void SimulationIsDeterministic()
    {
        object a = Build("a", 15f);
        object b = Build("b", 12f);
        object tuning = CreateTuning();

        object first = ProductionReflection.CallStatic(
            "BalanceSimulator", "Run", a, b, 25, null, tuning);

        object second = ProductionReflection.CallStatic(
            "BalanceSimulator", "Run", a, b, 25, null, tuning);

        Assert.AreEqual(Read(first, "WinsA"), Read(second, "WinsA"));
        Assert.AreEqual(Read(first, "WinsB"), Read(second, "WinsB"));
        Assert.AreEqual(Read(first, "Draws"), Read(second, "Draws"));
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

    private static object CreateTuning()
    {
        return ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("CombatTuning"));
    }

    private static int Read(object result, string field)
    {
        return (int)ProductionReflection.Field(result, field);
    }
}
