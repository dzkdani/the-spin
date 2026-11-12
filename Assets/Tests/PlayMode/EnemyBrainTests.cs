using NUnit.Framework;
using UnityEngine;

public class EnemyBrainTests
{
    [Test]
    public void AggressiveProfileRequestsDashInRange()
    {
        object aggressive = CreateProfile(5f, 1f);
        object passive = CreateProfile(0f, 0f);

        object aggressiveCommand = Decide(aggressive, CreateSnapshot(0.9f, 0.9f, 2f));
        object passiveCommand = Decide(passive, CreateSnapshot(0.9f, 0.9f, 2f));

        Assert.IsTrue((bool)ProductionReflection.Field(aggressiveCommand, "RequestDash"));
        Assert.IsFalse((bool)ProductionReflection.Field(passiveCommand, "RequestDash"));
    }

    [Test]
    public void RetreatMovesAwayFromOpponent()
    {
        object profile = CreateProfile(1f, 0.5f);
        object snapshot = CreateSnapshot(0.05f, 1f, 2f);

        object command = Decide(profile, snapshot);

        Vector2 self = (Vector2)ProductionReflection.Field(snapshot, "SelfPosition");
        Vector2 opponent = (Vector2)ProductionReflection.Field(snapshot, "OpponentPosition");
        Vector2 target = (Vector2)ProductionReflection.Field(command, "MoveTarget");

        Assert.Greater(
            Vector2.Distance(target, opponent),
            Vector2.Distance(self, opponent));
    }

    [Test]
    public void SameSeedProducesSameDecision()
    {
        object profile = CreateProfile(1f, 0.5f);
        object snapshot = CreateSnapshot(0.8f, 0.7f, 3f);

        object first = Decide(profile, snapshot, seed: 42);
        object second = Decide(profile, snapshot, seed: 42);

        Assert.AreEqual(
            (Vector2)ProductionReflection.Field(first, "MoveTarget"),
            (Vector2)ProductionReflection.Field(second, "MoveTarget"));

        Assert.AreEqual(
            (bool)ProductionReflection.Field(first, "RequestDash"),
            (bool)ProductionReflection.Field(second, "RequestDash"));
    }

    [Test]
    public void UseActiveRequestsWhenOpponentIsLow()
    {
        object profile = CreateProfile(0f, 0f);

        object low = Decide(profile, CreateSnapshot(0.8f, 0.2f, 2f));
        object high = Decide(profile, CreateSnapshot(0.8f, 0.9f, 2f));

        Assert.AreEqual(
            0,
            (int)ProductionReflection.Field(low, "AbilitySlot"));

        Assert.AreEqual(
            -1,
            (int)ProductionReflection.Field(high, "AbilitySlot"));
    }

    [Test]
    public void UseActiveDoesNotRequestWhenNoActiveIsReady()
    {
        object profile = CreateProfile(0f, 0f);

        object command = Decide(
            profile,
            CreateSnapshot(0.8f, 0.2f, 2f, activeReady: false));

        Assert.AreEqual(
            -1,
            (int)ProductionReflection.Field(command, "AbilitySlot"));
    }

    [Test]
    public void AggressiveProfileDashesMoreInSimulation()
    {
        object aggressive = CreateProfile(5f, 1f);
        object passive = CreateProfile(0f, 0f);

        object aggressiveRun = ProductionReflection.CallStatic(
            "AiSimulation", "Run", aggressive, 7, 40);

        object passiveRun = ProductionReflection.CallStatic(
            "AiSimulation", "Run", passive, 7, 40);

        Assert.Greater(
            (int)ProductionReflection.Field(aggressiveRun, "Dashes"),
            (int)ProductionReflection.Field(passiveRun, "Dashes"));

        Assert.Greater(
            (int)ProductionReflection.Field(passiveRun, "AbilityRequests"),
            0);
    }

    private static object Decide(object profile, object snapshot, int seed = 7)
    {
        object random = ProductionReflection.New("SeededRandom", seed);
        object brain = ProductionReflection.New("EnemyBrain", profile, random);

        ProductionReflection.Call(brain, "Tick", snapshot, 1f);

        return ProductionReflection.Call(brain, "Tick", snapshot, 1f);
    }

    private static object CreateProfile(float strikeWeight, float aggression)
    {
        object profile = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("AiProfile"));

        ProductionReflection.SetField(profile, "decisionInterval", 0.15f);
        ProductionReflection.SetField(profile, "commitTime", 0.6f);
        ProductionReflection.SetField(profile, "strikeWeight", strikeWeight);
        ProductionReflection.SetField(profile, "dashAggression", aggression);
        ProductionReflection.SetField(profile, "jitterRadius", 0f);
        ProductionReflection.SetField(profile, "retreatWeight", 2f);
        ProductionReflection.SetField(profile, "retreatRpm", 0.25f);
        ProductionReflection.SetField(profile, "strikeRange", 4f);
        ProductionReflection.SetField(profile, "activeWeight", 0.9f);

        return profile;
    }

    private static object CreateSnapshot(
        float selfRpm,
        float opponentRpm,
        float distance,
        bool activeReady = true)
    {
        return ProductionReflection.New(
            "BattleSnapshot",
            selfRpm,
            opponentRpm,
            Vector2.zero,
            new Vector2(distance, 0f),
            distance,
            null,
            null,
            false,
            1f,
            0f,
            new[] { activeReady },
            new[] { 0f });
    }
}
