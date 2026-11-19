using NUnit.Framework;
using UnityEngine;

public class BattleFlowTests
{
    [Test]
    public void PlayerDepletedLoses()
    {
        object condition = ProductionReflection.New("RpmDepletedCondition");
        object context = Context(0f, 0, 4);

        bool satisfied = (bool)ProductionReflection.Call(
            condition,
            "IsSatisfied",
            context);

        Assert.IsTrue(satisfied);
        Assert.AreEqual(
            "Lose",
            ProductionReflection.Property(condition, "Result").ToString());
    }

    [Test]
    public void EnemiesClearedWins()
    {
        object condition = ProductionReflection.New("EnemiesClearedCondition");

        Assert.IsTrue((bool)ProductionReflection.Call(
            condition,
            "IsSatisfied",
            Context(100f, 4, 4)));

        Assert.IsFalse((bool)ProductionReflection.Call(
            condition,
            "IsSatisfied",
            Context(100f, 3, 4)));

        Assert.AreEqual(
            "Win",
            ProductionReflection.Property(condition, "Result").ToString());
    }

    [Test]
    public void ControllerResolvesLoseBeforeWin()
    {
        object controller = ProductionReflection.New("BattleController");

        ProductionReflection.Call(
            controller,
            "AddCondition",
            ProductionReflection.New("RpmDepletedCondition"));

        ProductionReflection.Call(
            controller,
            "AddCondition",
            ProductionReflection.New("EnemiesClearedCondition"));

        object result = ProductionReflection.Call(
            controller,
            "Evaluate",
            Context(0f, 4, 4));

        Assert.IsNotNull(result);
        Assert.AreEqual("Lose", result.ToString());
        Assert.IsTrue((bool)ProductionReflection.Property(controller, "Finished"));
    }

    [Test]
    public void ControllerStaysOpenMidBattle()
    {
        object controller = ProductionReflection.New("BattleController");

        ProductionReflection.Call(
            controller,
            "AddCondition",
            ProductionReflection.New("RpmDepletedCondition"));

        ProductionReflection.Call(
            controller,
            "AddCondition",
            ProductionReflection.New("EnemiesClearedCondition"));

        object result = ProductionReflection.Call(
            controller,
            "Evaluate",
            Context(100f, 1, 4));

        Assert.IsNull(result);
        Assert.IsFalse((bool)ProductionReflection.Property(controller, "Finished"));
    }

    [Test]
    public void PlayerOutsideArenaLoses()
    {
        object condition = ProductionReflection.New("RingOutCondition");

        object outside = ProductionReflection.New(
            "BattleContext",
            100f,
            0,
            4,
            new Vector2(9f, 0f),
            new Vector2(8f, 8f));

        object inside = ProductionReflection.New(
            "BattleContext",
            100f,
            0,
            4,
            new Vector2(1f, 0f),
            new Vector2(8f, 8f));

        Assert.IsTrue((bool)ProductionReflection.Call(
            condition,
            "IsSatisfied",
            outside));

        Assert.IsFalse((bool)ProductionReflection.Call(
            condition,
            "IsSatisfied",
            inside));

        Assert.AreEqual(
            "Lose",
            ProductionReflection.Property(condition, "Result").ToString());

        Assert.AreEqual(
            "RingOut",
            ProductionReflection.Property(condition, "Reason").ToString());
    }

    [Test]
    public void PlayerBurstLoses()
    {
        object condition = ProductionReflection.New("BurstCondition", true);

        object context = ProductionReflection.New(
            "BattleContext",
            100f,
            0,
            4,
            Vector2.zero,
            new Vector2(8f, 8f),
            true,
            false);

        Assert.IsTrue((bool)ProductionReflection.Call(
            condition,
            "IsSatisfied",
            context));

        Assert.AreEqual(
            "Lose",
            ProductionReflection.Property(condition, "Result").ToString());

        Assert.AreEqual(
            "Burst",
            ProductionReflection.Property(condition, "Reason").ToString());
    }

    [Test]
    public void EnemyBurstWins()
    {
        object condition = ProductionReflection.New("BurstCondition", false);

        object context = ProductionReflection.New(
            "BattleContext",
            100f,
            0,
            4,
            Vector2.zero,
            new Vector2(8f, 8f),
            false,
            true);

        Assert.IsTrue((bool)ProductionReflection.Call(
            condition,
            "IsSatisfied",
            context));

        Assert.AreEqual(
            "Win",
            ProductionReflection.Property(condition, "Result").ToString());
    }

    [Test]
    public void NoBurstDoesNotFinish()
    {
        object condition = ProductionReflection.New("BurstCondition", false);

        Assert.IsFalse((bool)ProductionReflection.Call(
            condition,
            "IsSatisfied",
            Context(100f, 0, 4)));
    }

    [Test]
    public void BattleControllerFinishSetsResultAndReason()
    {
        object controller = ProductionReflection.New("BattleController");

        ProductionReflection.Call(
            controller,
            "Finish",
            ProductionReflection.EnumValue("GameResult", "Win"),
            ProductionReflection.EnumValue("FinishReason", "RingOut"));

        Assert.IsTrue((bool)ProductionReflection.Property(controller, "Finished"));
        Assert.AreEqual(
            "Win",
            ProductionReflection.Property(controller, "Result").ToString());
        Assert.AreEqual(
            "RingOut",
            ProductionReflection.Property(controller, "Reason").ToString());
    }

    private static object Context(
        float playerRpm,
        int enemiesDefeated,
        int totalEnemies)
    {
        return ProductionReflection.New(
            "BattleContext",
            playerRpm,
            enemiesDefeated,
            totalEnemies);
    }
}
