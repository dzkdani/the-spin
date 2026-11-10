using NUnit.Framework;

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
