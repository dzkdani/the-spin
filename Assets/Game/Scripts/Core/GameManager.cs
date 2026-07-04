using UnityEngine;
using System.Collections;

public enum GameResult
{
    Win,
    Lose
}

public class GameManager : MonoBehaviour
{
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
    private bool finished;

    private void Start()
    {
        if (player == null || player.Data == null)
        {
            Debug.LogError("Player or Player.Data is not assigned.");
            return;
        }

        player.Data.OnRPMDepleted += PlayerLose;
        playerUI.Bind(player.Data);

        SpawnNextEnemy();
    }

    private void SpawnNextEnemy()
    {
        BeyController enemy = spawner.SpawnEnemy();

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

        GameUIManager.Instance.ShowGameOver(GameResult.Win);
        
        Debug.Log("YOU WIN");
    }

    private void PlayerLose()
    {
        if (finished)
            return;

        finished = true;

        GameUIManager.Instance.ShowGameOver(GameResult.Lose);

        Debug.Log("YOU LOSE");
    }

    private void Update()
    {
        if (!finished)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //pause game
        }
    }

    private void OnDestroy()
    {
        if (player != null)
            player.Data.OnRPMDepleted -= PlayerLose;

        if (currentEnemy != null)
            currentEnemy.OnRPMDepleted -= EnemyDefeated;
    }
}