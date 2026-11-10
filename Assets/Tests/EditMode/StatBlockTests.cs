using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;

public class StatBlockTests
{
    [Test]
    public void FormulaCombinesAddPercentAndMultiply()
    {
        object stats = ProductionReflection.New("StatBlock");
        SetBase(stats, "Attack", 10f);

        Add(stats, "Attack", "Add", 5f, "mod.add");
        Add(stats, "Attack", "PercentAdd", 0.5f, "mod.percent");
        Add(stats, "Attack", "Multiply", 2f, "mod.multiply");

        Assert.AreEqual(45f, Get(stats, "Attack"), 0.0001f);
    }

    [Test]
    public void ResultDoesNotDependOnApplicationOrder()
    {
        object forward = ProductionReflection.New("StatBlock");
        object reverse = ProductionReflection.New("StatBlock");

        SetBase(forward, "Attack", 10f);
        SetBase(reverse, "Attack", 10f);

        Add(forward, "Attack", "Add", 5f, "a");
        Add(forward, "Attack", "PercentAdd", 0.5f, "b");
        Add(forward, "Attack", "Multiply", 2f, "c");

        Add(reverse, "Attack", "Multiply", 2f, "c");
        Add(reverse, "Attack", "PercentAdd", 0.5f, "b");
        Add(reverse, "Attack", "Add", 5f, "a");

        Assert.AreEqual(
            Get(forward, "Attack"),
            Get(reverse, "Attack"),
            0.0001f);
    }

    [Test]
    public void ScopedReadsIncludeGlobalAndMatchingScopeOnly()
    {
        object stats = ProductionReflection.New("StatBlock");
        SetBase(stats, "AbilityCost", 0.2f);

        Add(stats, "AbilityCost", "Multiply", 0.9f, "global");
        AddScoped(stats, "AbilityCost", "Multiply", 0.8f, "dash", "dash-only");

        Assert.AreEqual(0.18f, Get(stats, "AbilityCost"), 0.0001f);
        Assert.AreEqual(0.144f, GetScoped(stats, "AbilityCost", "dash"), 0.0001f);
        Assert.AreEqual(0.18f, GetScoped(stats, "AbilityCost", "guard"), 0.0001f);
    }

    [Test]
    public void ScopeOnNonScopedStatIsRejected()
    {
        object stats = ProductionReflection.New("StatBlock");

        Assert.Throws<TargetInvocationException>(() =>
            AddScoped(stats, "Attack", "Multiply", 2f, "dash", "bad"));
    }

    [Test]
    public void RefreshStackingResetsTimerWithoutStacking()
    {
        object stats = ProductionReflection.New("StatBlock");
        SetBase(stats, "Attack", 1f);

        AddWithTiming(stats, "Attack", "Multiply", 2f, "buff", 5f, "Refresh", 1);
        AddWithTiming(stats, "Attack", "Multiply", 3f, "buff", 10f, "Refresh", 1);

        Assert.AreEqual(2f, Get(stats, "Attack"), 0.0001f);
        Assert.AreEqual(1, ExplainCount(stats, "Attack"));

        ProductionReflection.Call(stats, "Tick", 6f);

        Assert.AreEqual(2f, Get(stats, "Attack"), 0.0001f);
    }

    [Test]
    public void IgnoreStackingKeepsFirstApplication()
    {
        object stats = ProductionReflection.New("StatBlock");
        SetBase(stats, "Attack", 1f);

        AddWithTiming(stats, "Attack", "Multiply", 2f, "buff", 10f, "Ignore", 1);
        AddWithTiming(stats, "Attack", "Multiply", 3f, "buff", 10f, "Ignore", 1);

        Assert.AreEqual(2f, Get(stats, "Attack"), 0.0001f);
    }

    [Test]
    public void StackStackingStopsAtMaxStacks()
    {
        object stats = ProductionReflection.New("StatBlock");
        SetBase(stats, "Attack", 1f);

        AddWithTiming(stats, "Attack", "Multiply", 2f, "buff", 10f, "Stack", 2);
        AddWithTiming(stats, "Attack", "Multiply", 2f, "buff", 10f, "Stack", 2);
        AddWithTiming(stats, "Attack", "Multiply", 2f, "buff", 10f, "Stack", 2);

        Assert.AreEqual(4f, Get(stats, "Attack"), 0.0001f);
    }

    [Test]
    public void TickRemovesExpiredModifiers()
    {
        object stats = ProductionReflection.New("StatBlock");
        SetBase(stats, "Attack", 1f);

        AddWithTiming(stats, "Attack", "Multiply", 2f, "buff", 5f, "Refresh", 1);
        ProductionReflection.Call(stats, "Tick", 6f);

        Assert.AreEqual(1f, Get(stats, "Attack"), 0.0001f);
    }

    [Test]
    public void RemoveBySourceClearsEveryContribution()
    {
        object stats = ProductionReflection.New("StatBlock");
        SetBase(stats, "Attack", 1f);
        SetBase(stats, "Weight", 1f);

        Add(stats, "Attack", "Multiply", 2f, "part:heavy");
        Add(stats, "Weight", "Add", 3f, "part:heavy");
        Add(stats, "Attack", "Multiply", 4f, "part:light");

        ProductionReflection.Call(stats, "RemoveBySource", "part:heavy");

        Assert.AreEqual(4f, Get(stats, "Attack"), 0.0001f);
        Assert.AreEqual(1f, Get(stats, "Weight"), 0.0001f);
        Assert.AreEqual(1, ExplainCount(stats, "Attack"));
    }

    private static int ExplainCount(object stats, string stat)
    {
        IEnumerable contributions = (IEnumerable)ProductionReflection.Call(
            stats,
            "Explain",
            ProductionReflection.EnumValue("StatId", stat),
            null);

        int count = 0;

        foreach (object contribution in contributions)
            count++;

        return count;
    }

    private static void SetBase(object stats, string stat, float value)
    {
        ProductionReflection.Call(
            stats,
            "SetBase",
            ProductionReflection.EnumValue("StatId", stat),
            value);
    }

    private static float Get(object stats, string stat)
    {
        return (float)ProductionReflection.Call(
            stats,
            "Get",
            ProductionReflection.EnumValue("StatId", stat),
            null);
    }

    private static float GetScoped(object stats, string stat, string scope)
    {
        return (float)ProductionReflection.Call(
            stats,
            "Get",
            ProductionReflection.EnumValue("StatId", stat),
            scope);
    }

    private static void Add(object stats, string stat, string op, float value, string source)
    {
        AddWithTiming(stats, stat, op, value, source, float.PositiveInfinity, "Refresh", 1);
    }

    private static void AddScoped(
        object stats,
        string stat,
        string op,
        float value,
        string scope,
        string source)
    {
        AddModifier(stats, stat, op, value, source, scope, float.PositiveInfinity, "Refresh", 1);
    }

    private static void AddWithTiming(
        object stats,
        string stat,
        string op,
        float value,
        string source,
        float expiresAt,
        string stacking,
        int maxStacks)
    {
        AddModifier(stats, stat, op, value, source, null, expiresAt, stacking, maxStacks);
    }

    private static void AddModifier(
        object stats,
        string stat,
        string op,
        float value,
        string source,
        string scope,
        float expiresAt,
        string stacking,
        int maxStacks)
    {
        object modifier = ProductionReflection.New(
            "StatModifier",
            ProductionReflection.EnumValue("StatId", stat),
            ProductionReflection.EnumValue("ModOp", op),
            value,
            source,
            scope,
            expiresAt,
            ProductionReflection.EnumValue("StackPolicy", stacking),
            maxStacks);

        ProductionReflection.Call(stats, "Add", modifier, 0f);
    }
}
