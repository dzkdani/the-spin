using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class ImpactRuleTests
{
    [Test]
    public void SpinStealRuleGainsOnOppositeSpin()
    {
        object state = State();
        object input = Input(oppositeSpin: true, dashingA: false, dashingB: false);
        object rule = ProductionReflection.New("SpinStealRule");

        ProductionReflection.Call(rule, "Apply", state, input);

        Assert.AreEqual(10f, (float)ProductionReflection.Field(state, "StealToA"), 0.0001f);
        Assert.AreEqual(10f, (float)ProductionReflection.Field(state, "StealToB"), 0.0001f);
    }

    [Test]
    public void MatchupScaleSoftensAdvantageAndDisadvantage()
    {
        object pyre = Load("Assets/Game/Datas/Pyre.asset");
        object crye = Load("Assets/Game/Datas/Crye.asset");
        object table = Load("Assets/Game/Datas/MatchupTable.asset");

        object input = ProductionReflection.New("ImpactInput");

        ProductionReflection.SetField(input, "ElementA", pyre);
        ProductionReflection.SetField(input, "ElementB", crye);
        ProductionReflection.SetField(input, "Matchups", table);
        ProductionReflection.SetField(input, "StatsA", Stats(0.6f));
        ProductionReflection.SetField(input, "StatsB", Stats(0.6f));

        object state = State();
        object rule = ProductionReflection.New("MatchupRule");

        ProductionReflection.Call(rule, "Apply", state, input);

        Assert.AreEqual(121f, Read(state, "PowerA"), 0.0001f);
        Assert.AreEqual(85f, Read(state, "PowerB"), 0.0001f);
    }

    [Test]
    public void SameSpinRuleMultipliesBothSides()
    {
        object state = State();
        object input = Input(oppositeSpin: false, dashingA: false, dashingB: false);
        object rule = ProductionReflection.New("SameSpinRule");

        ProductionReflection.Call(rule, "Apply", state, input);

        Assert.AreEqual(110f, Read(state, "PowerA"), 0.0001f);
        Assert.AreEqual(110f, Read(state, "PowerB"), 0.0001f);
    }

    [Test]
    public void DashHitRuleBoostsOnlyDashingSide()
    {
        object state = State();
        object input = Input(oppositeSpin: true, dashingA: true, dashingB: false);
        object rule = ProductionReflection.New("DashHitRule");

        ProductionReflection.Call(rule, "Apply", state, input);

        Assert.AreEqual(125f, Read(state, "PowerA"), 0.0001f);
        Assert.AreEqual(100f, Read(state, "PowerB"), 0.0001f);
    }

    [Test]
    public void ClashRuleBoostsWhenBothDash()
    {
        object state = State();
        object input = Input(oppositeSpin: true, dashingA: true, dashingB: true);
        object rule = ProductionReflection.New("ClashRule");

        ProductionReflection.Call(rule, "Apply", state, input);

        Assert.AreEqual(150f, Read(state, "PowerA"), 0.0001f);
        Assert.AreEqual(150f, Read(state, "PowerB"), 0.0001f);
    }

    private static object Stats(float matchupScale)
    {
        object stats = ProductionReflection.New("StatBlock");

        ProductionReflection.Call(
            stats,
            "SetBase",
            ProductionReflection.EnumValue("StatId", "MatchupScale"),
            matchupScale);

        return stats;
    }

    private static object Load(string path)
    {
        object asset = AssetDatabase.LoadAssetAtPath(
            path,
            typeof(ScriptableObject));

        Assert.IsNotNull(asset, $"Missing asset: {path}");

        return asset;
    }

    private static object State()
    {
        object state = ProductionReflection.New("ImpactState");

        ProductionReflection.SetField(state, "PowerA", 100f);
        ProductionReflection.SetField(state, "PowerB", 100f);

        return state;
    }

    private static object Input(
        bool oppositeSpin,
        bool dashingA,
        bool dashingB)
    {
        object tuning = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("CombatTuning"));

        ProductionReflection.SetField(tuning, "sameSpinMultiplier", 1.1f);
        ProductionReflection.SetField(tuning, "dashHitMultiplier", 1.25f);
        ProductionReflection.SetField(tuning, "clashMultiplier", 1.5f);
        ProductionReflection.SetField(tuning, "spinStealRate", 0.1f);

        object input = ProductionReflection.New("ImpactInput");

        ProductionReflection.SetField(input, "OppositeSpin", oppositeSpin);
        ProductionReflection.SetField(input, "DashingA", dashingA);
        ProductionReflection.SetField(input, "DashingB", dashingB);
        ProductionReflection.SetField(input, "Tuning", tuning);

        return input;
    }

    private static float Read(object state, string field)
    {
        return (float)ProductionReflection.Field(state, field);
    }
}
