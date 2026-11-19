using NUnit.Framework;
using UnityEngine;

public class RetreatBehaviorTests
{
    private const float ArenaRadius = 8f;

    [Test]
    public void RetreatOrbitsAroundArenaCenter()
    {
        object profile = CreateProfile(1f);
        object snapshot = CreateSnapshot(new Vector2(5f, 0f), Vector2.zero);

        Vector2 target = Decide(profile, snapshot);

        Assert.Greater(
            Mathf.Abs(target.y),
            0.5f,
            "Retreat should move along a tangent, not straight away.");
    }

    [Test]
    public void RetreatNearEdgeBiasMovesInward()
    {
        object profile = CreateProfile(1f);
        Vector2 self = new Vector2(7.5f, 0f);
        object snapshot = CreateSnapshot(self, Vector2.zero);

        Vector2 target = Decide(profile, snapshot);

        Assert.Less(target.magnitude, self.magnitude);
    }

    [Test]
    public void RetreatTargetStaysInsideArena()
    {
        object profile = CreateProfile(1f);
        object snapshot = CreateSnapshot(new Vector2(7.9f, 0f), Vector2.zero);

        Vector2 target = Decide(profile, snapshot);

        Assert.LessOrEqual(target.magnitude, ArenaRadius - 0.8f + 0.0001f);
    }

    [Test]
    public void RetreatIgnoresJitter()
    {
        object profile = CreateProfile(1f);
        ProductionReflection.SetField(profile, "jitterRadius", 3f);
        object snapshot = CreateSnapshot(new Vector2(5f, 0f), Vector2.zero);

        object random = ProductionReflection.New("SeededRandom", 9);
        object brain = ProductionReflection.New("EnemyBrain", profile, random);
        ProductionReflection.Call(brain, "Tick", snapshot, 0.15f);

        object command = ProductionReflection.Field(brain, "command");

        Assert.AreEqual(
            Decide(profile, snapshot),
            (Vector2)ProductionReflection.Field(command, "MoveTarget"));
    }

    [Test]
    public void RetreatOrbitZeroFallsBackToDefault()
    {
        object profile = CreateProfile(0f);
        object snapshot = CreateSnapshot(new Vector2(5f, 0f), Vector2.zero);

        Vector2 target = Decide(profile, snapshot);

        Assert.Greater(
            Mathf.Abs(target.y),
            0.5f,
            "An unserialized orbit value must still produce tangential drift.");
    }

    [Test]
    public void RetreatSpiralsInwardAsRpmFalls()
    {
        object profile = CreateProfile(1f);
        object weak = CreateSnapshot(
            new Vector2(5f, 0f), Vector2.zero, selfRpm: 0.14f);
        object dying = CreateSnapshot(
            new Vector2(5f, 0f), Vector2.zero, selfRpm: 0.01f);

        Assert.Less(
            Decide(profile, dying).x,
            Decide(profile, weak).x);
    }

    [Test]
    public void RetreatCooldownBlocksImmediateRetry()
    {
        object behavior = ProductionReflection.New("RetreatBehavior");
        object profile = CreateProfile(1f);
        object snapshot = CreateSnapshot(new Vector2(5f, 0f), Vector2.zero);

        ProductionReflection.Call(behavior, "Decide", snapshot, profile);

        Assert.AreEqual(
            0f,
            (float)ProductionReflection.Call(behavior, "Score", snapshot, profile),
            0.0001f);

        object later = CreateSnapshot(
            new Vector2(5f, 0f), Vector2.zero, now: 2.1f);

        Assert.Greater(
            (float)ProductionReflection.Call(behavior, "Score", later, profile),
            0f);
    }

    private static Vector2 Decide(object profile, object snapshot)
    {
        object behavior = ProductionReflection.New("RetreatBehavior");
        object command = ProductionReflection.Call(
            behavior, "Decide", snapshot, profile);

        return (Vector2)ProductionReflection.Field(command, "MoveTarget");
    }

    private static object CreateProfile(float orbit)
    {
        object profile = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("AiProfile"));

        ProductionReflection.SetField(profile, "retreatOrbit", orbit);
        ProductionReflection.SetField(profile, "retreatRpm", 0.15f);
        ProductionReflection.SetField(profile, "retreatWeight", 2f);

        return profile;
    }

    private static object CreateSnapshot(
        Vector2 self,
        Vector2 opponent,
        float selfRpm = 0.05f,
        float now = 0f)
    {
        return ProductionReflection.New(
            "BattleSnapshot",
            selfRpm,
            1f,
            self,
            opponent,
            Vector2.Distance(self, opponent),
            null,
            null,
            false,
            1f,
            now,
            System.Array.Empty<bool>(),
            System.Array.Empty<float>(),
            Vector2.zero,
            ArenaRadius);
    }
}
