using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GameResult
{
    Win,
    Lose
}

public class GameManager : MonoBehaviour
{
    [Header("Content")]
    [SerializeField] private ContentManifest contentManifest;

    [Header("Player")]
    [SerializeField] private BeyController player;

    [Header("Enemy")]
    [SerializeField] private EnemySpawner spawner;
    [SerializeField] private int totalEnemies = 2;

    [Header("UI")]
    [SerializeField] private BeyUI playerUI;
    [SerializeField] private BeyUI enemyUI;

    public int CurrentEnemyIndex => enemiesDefeated + 1;
    public int TotalEnemies => totalEnemies;

    private int enemiesDefeated;
    private BeyData currentEnemy;
    private BeyData subscribedPlayer;
    private BattleController battle;

    private void Awake()
    {
        ContentRegistry.Initialize(contentManifest);

        List<string> errors = ContentValidator.Validate(contentManifest);

        for (int i = 0; i < errors.Count; i++)
            Debug.LogError("Content: " + errors[i], this);
    }

    private void Start()
    {
        if (player == null || player.Data == null || spawner == null ||
            playerUI == null || enemyUI == null || totalEnemies <= 0)
        {
            Debug.LogError(
                "GameManager requires a player with BeyData, an EnemySpawner, both BeyUIs, and at least one enemy.",
                this);
            return;
        }

        subscribedPlayer = player.Data;

        battle = new BattleController();
        battle.AddCondition(new RpmDepletedCondition());
        battle.AddCondition(new EnemiesClearedCondition());

        subscribedPlayer.OnRPMDepleted -= ResolveBattle;
        subscribedPlayer.OnRPMDepleted += ResolveBattle;
        playerUI.Bind(player.Data);

        SpawnNextEnemy();
    }

    private void SpawnNextEnemy()
    {
        if (spawner == null || enemyUI == null)
        {
            Debug.LogError("GameManager cannot spawn an enemy because its spawner or enemy UI is missing.", this);
            return;
        }

        BeyController enemy = spawner.SpawnEnemy();

        if (enemy == null || enemy.Data == null)
        {
            Debug.LogError("EnemySpawner returned an enemy without a BeyController or BeyData.", this);
            return;
        }

        currentEnemy = enemy.Data;

        if (enemy.Input is EnemyInput enemyInput)
            enemyInput.SetTarget(player);

        enemyUI.Bind(currentEnemy);

        currentEnemy.OnRPMDepleted += EnemyDefeated;
    }

    private void EnemyDefeated()
    {
        currentEnemy.OnRPMDepleted -= EnemyDefeated;

        enemyUI.Bind(null);

        Destroy(currentEnemy.gameObject);

        if (battle == null || battle.Finished)
            return;

        enemiesDefeated++;

        ResolveBattle();

        if (battle.Finished)
            return;

        StartCoroutine(SpawnNextAfterDelay(1f));
    }

    private IEnumerator SpawnNextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        SpawnNextEnemy();
    }

    private void ResolveBattle()
    {
        if (battle == null || battle.Finished)
            return;

        BattleContext context = new BattleContext(
            subscribedPlayer != null ? subscribedPlayer.CurrentRPM : 0f,
            enemiesDefeated,
            totalEnemies);

        GameResult? result = battle.Evaluate(context);

        if (!result.HasValue)
            return;

        ServiceLocator.Get<GameUIManager>()?.ShowGameOver(result.Value);
    }

    private void OnDestroy()
    {
        if (subscribedPlayer != null)
            subscribedPlayer.OnRPMDepleted -= ResolveBattle;

        if (currentEnemy != null)
            currentEnemy.OnRPMDepleted -= EnemyDefeated;
    }
}
