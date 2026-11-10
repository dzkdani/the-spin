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
    private bool finished;

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
        subscribedPlayer.OnRPMDepleted -= PlayerLose;
        subscribedPlayer.OnRPMDepleted += PlayerLose;
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

        enemyUI.Bind(currentEnemy);

        currentEnemy.OnRPMDepleted += EnemyDefeated;
    }

    private void EnemyDefeated()
    {
        currentEnemy.OnRPMDepleted -= EnemyDefeated;

        enemyUI.Bind(null);

        Destroy(currentEnemy.gameObject);

        if (finished)
            return;

        enemiesDefeated++;

        if (enemiesDefeated >= totalEnemies)
        {
            PlayerWin();
            return;
        }

        StartCoroutine(SpawnNextAfterDelay(1f));
    }

    private IEnumerator SpawnNextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        SpawnNextEnemy();
    }

    private void PlayerWin()
    {
        if (finished)
            return;

        finished = true;

        ServiceLocator.Get<GameUIManager>()?.ShowGameOver(GameResult.Win);
        
        Debug.Log("YOU WIN");
    }

    private void PlayerLose()
    {
        if (finished)
            return;

        finished = true;

        ServiceLocator.Get<GameUIManager>()?.ShowGameOver(GameResult.Lose);

        Debug.Log("YOU LOSE");
    }

    private void OnDestroy()
    {
        if (subscribedPlayer != null)
            subscribedPlayer.OnRPMDepleted -= PlayerLose;

        if (currentEnemy != null)
            currentEnemy.OnRPMDepleted -= EnemyDefeated;
    }
}
