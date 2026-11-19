using NUnit.Framework;
using UnityEngine;

public class RingOutFlowTests
{
    [Test]
    public void PlayerOutsideArenaFinishesLoseWithRingOutReason()
    {
        object battle = ProductionReflection.New("BattleController");

        ProductionReflection.Call(
            battle, "AddCondition",
            ProductionReflection.New("RingOutCondition"));

        GameObject arenaObject = new GameObject("Arena");
        arenaObject.SetActive(false);

        GameObject playerObject = new GameObject("Player");
        playerObject.SetActive(false);
        playerObject.transform.position = new Vector3(20f, 0f, 0f);

        Component player = playerObject.AddComponent(
            ProductionReflection.TypeOf("BeyController"));

        Component manager = NewManager(arenaObject, battle, null, 6);

        try
        {
            ProductionReflection.SetField(manager, "player", player);
            ProductionReflection.CallPrivate(manager, "CheckRingOut");

            Assert.IsTrue((bool)ProductionReflection.Property(battle, "Finished"));
            Assert.AreEqual(
                "Lose",
                ProductionReflection.Property(battle, "Result").ToString());
            Assert.AreEqual(
                "RingOut",
                ProductionReflection.Property(battle, "Reason").ToString());
        }
        finally
        {
            Object.DestroyImmediate(manager.gameObject);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(arenaObject);
        }
    }

    [Test]
    public void EnemyOutsideArenaOnLastWaveFinishesWinWithRingOutReason()
    {
        object battle = ProductionReflection.New("BattleController");

        GameObject arenaObject = new GameObject("Arena");
        arenaObject.SetActive(false);

        GameObject enemyObject = new GameObject("Enemy");
        enemyObject.SetActive(false);
        enemyObject.transform.position = new Vector3(20f, 0f, 0f);

        Component enemy = enemyObject.AddComponent(
            ProductionReflection.TypeOf("BeyData"));

        ProductionReflection.SetField(
            enemy, "<Base>k__BackingField", BeyStats());

        Component manager = NewManager(arenaObject, battle, enemy, 1);

        try
        {
            ProductionReflection.CallPrivate(manager, "CheckRingOut");

            Assert.IsTrue((bool)ProductionReflection.Property(battle, "Finished"));
            Assert.AreEqual(
                "Win",
                ProductionReflection.Property(battle, "Result").ToString());
            Assert.AreEqual(
                "RingOut",
                ProductionReflection.Property(battle, "Reason").ToString());
        }
        finally
        {
            Object.DestroyImmediate(manager.gameObject);
            Object.DestroyImmediate(enemyObject);
            Object.DestroyImmediate(arenaObject);
        }
    }

    private static Component NewManager(
        GameObject arenaObject,
        object battle,
        Component enemy,
        int totalEnemies)
    {
        GameObject managerObject = new GameObject("Manager");
        managerObject.SetActive(false);

        Component manager = managerObject.AddComponent(
            ProductionReflection.TypeOf("GameManager"));

        ProductionReflection.SetField(
            manager,
            "arena",
            arenaObject.AddComponent(
                ProductionReflection.TypeOf("ArenaBounds")));

        ProductionReflection.SetField(manager, "battle", battle);
        ProductionReflection.SetField(manager, "currentEnemy", enemy);
        ProductionReflection.SetField(manager, "totalEnemies", totalEnemies);

        return manager;
    }

    private static object BeyStats()
    {
        object stats = ScriptableObject.CreateInstance(
            ProductionReflection.TypeOf("BeyStats"));

        ProductionReflection.SetField(stats, "maxRPM", 1000f);
        ProductionReflection.SetField(stats, "attack", 10f);
        ProductionReflection.SetField(stats, "weight", 5f);

        return stats;
    }
}
