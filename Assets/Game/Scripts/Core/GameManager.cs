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
    [SerializeField] private float arenaRadius = 8f;

    [Header("UI")]
    [SerializeField] private BeyUI playerUI;
    [SerializeField] private BeyUI enemyUI;

    [Header("Demo")]
    [SerializeField] private bool resetPlayerBetweenWaves = true;

    public int CurrentEnemyIndex => enemiesDefeated + 1;
    public int TotalEnemies => totalEnemies;

    public BeyController Player => player;
    public BeyData CurrentEnemy => currentEnemy;

    public int Wave =>
        Mathf.Min(enemiesDefeated + 1, totalEnemies);

    public FinishReason? LastFinishReason =>
        battle != null ? battle.Reason : null;

    public float PlayerRpm =>
        subscribedPlayer != null ? subscribedPlayer.CurrentRPM : 0f;

    public float WaveResetAt => waveResetAt;

    private float waveResetAt = -100f;

    private int enemiesDefeated;
    private BeyData currentEnemy;
    private BeyData subscribedPlayer;
    private BattleController battle;
    private bool playerBurst;
    private bool enemyBurst;

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
        battle.AddCondition(new BurstCondition(true));
        battle.AddCondition(new BurstCondition(false));
        battle.AddCondition(new RpmDepletedCondition());
        battle.AddCondition(new RingOutCondition());
        battle.AddCondition(new EnemiesClearedCondition());

        BattleEvents.ImpactPending -= OnImpactPending;
        BattleEvents.ImpactPending += OnImpactPending;

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

        BeyController enemy = spawner.SpawnEnemy(enemiesDefeated);

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

        ResetPlayerForNextWave();

        StartCoroutine(SpawnNextAfterDelay(1f));
    }

    private void ResetPlayerForNextWave()
    {
        if (!resetPlayerBetweenWaves ||
            player == null ||
            player.Data == null)
        {
            return;
        }

        player.Data.RestoreRPM();
        player.Data.Stats.RemoveBySourcePrefix("element:");
        player.transform.position = Vector3.zero;

        if (player.Physics != null &&
            player.Physics.Rigidbody != null)
        {
            player.Physics.Rigidbody.linearVelocity = Vector2.zero;
        }

        waveResetAt = Time.time;
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
            totalEnemies,
            player != null
                ? (Vector2)player.transform.position
                : Vector2.zero,
            arenaRadius,
            playerBurst,
            enemyBurst);

        GameResult? result = battle.Evaluate(context);

        if (!result.HasValue)
            return;

        ServiceLocator.Get<GameUIManager>()?.ShowGameOver(
            result.Value,
            battle.Reason ?? FinishReason.RpmDepleted);
    }

    private void OnDestroy()
    {
        BattleEvents.ImpactPending -= OnImpactPending;

        if (subscribedPlayer != null)
            subscribedPlayer.OnRPMDepleted -= ResolveBattle;

        if (currentEnemy != null)
            currentEnemy.OnRPMDepleted -= EnemyDefeated;
    }

    private void OnImpactPending(ImpactEvent impact)
    {
        if (subscribedPlayer == null)
            return;

        if (impact.Defender == subscribedPlayer)
        {
            playerBurst |= IsBurst(
                impact.Result.DamageToB,
                subscribedPlayer);
        }
        else if (impact.Attacker == subscribedPlayer)
        {
            playerBurst |= IsBurst(
                impact.Result.DamageToA,
                subscribedPlayer);
        }

        if (currentEnemy != null)
        {
            if (impact.Defender == currentEnemy)
            {
                enemyBurst |= IsBurst(
                    impact.Result.DamageToB,
                    currentEnemy);
            }
            else if (impact.Attacker == currentEnemy)
            {
                enemyBurst |= IsBurst(
                    impact.Result.DamageToA,
                    currentEnemy);
            }
        }

        if (playerBurst || enemyBurst)
            ResolveBattle();
    }

    public const float BurstThreshold = 0.8f;

    public static bool IsBurst(float damage, BeyData target)
    {
        return target != null &&
            target.Base != null &&
            target.Base.maxRPM > 0f &&
            damage >= target.Base.maxRPM * BurstThreshold;
    }
}
