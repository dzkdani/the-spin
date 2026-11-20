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

    [Header("Arena")]
    [SerializeField] private ArenaBounds arena;

    [Header("Launch")]
    [SerializeField] private LaunchController launch;

    [Header("UI")]
    [SerializeField] private BeyUI playerUI;
    [SerializeField] private BeyUI enemyUI;

    private GarageUI garage;

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
    private float ringOutTimer;

    private const float RingOutCheckInterval = 0.05f;

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

        BeginWave();
    }

    private void BeginWave()
    {
        if (launch != null && launch.Enabled)
        {
            launch.Begin(player, arena, SpawnNextEnemy);
            return;
        }

        ResetPlayerForNextWave();
        SpawnNextEnemy();
    }

    private void Update()
    {
        if (battle == null || battle.Finished || arena == null ||
            Time.timeScale <= 0f)
            return;

        ringOutTimer -= Time.deltaTime;

        if (ringOutTimer > 0f)
            return;

        ringOutTimer = RingOutCheckInterval;
        CheckRingOut();
    }

    private void CheckRingOut()
    {
        if (player != null && player.transform != null)
        {
            Vector2 playerPos = player.transform.position;

            if (ArenaBounds.NormalizedDistance(
                    playerPos, arena.Center, arena.Radii) > 1f)
            {
                ResolveBattle();
                return;
            }
        }

        if (currentEnemy == null || currentEnemy.transform == null)
            return;

        Vector2 enemyPos = currentEnemy.transform.position;

        if (ArenaBounds.NormalizedDistance(
                enemyPos, arena.Center, arena.Radii) <= 1f)
            return;

        if (CurrentEnemyIndex >= totalEnemies)
        {
            battle.Finish(GameResult.Win, FinishReason.RingOut);

            ServiceLocator.Get<GameUIManager>()?.ShowGameOver(
                GameResult.Win,
                FinishReason.RingOut);

            return;
        }

        DefeatEnemy();
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
        {
            enemyInput.SetTarget(player);

            if (arena != null)
                enemyInput.SetArena(arena.Center, arena.Radii);
        }

        if (launch != null)
            launch.ApplyEnemyLaunch(currentEnemy, enemiesDefeated);

        enemyUI.Bind(currentEnemy);

        currentEnemy.OnRPMDepleted += EnemyDefeated;
    }

    private void EnemyDefeated()
    {
        if (currentEnemy == null)
            return;

        currentEnemy.OnRPMDepleted -= EnemyDefeated;
        DefeatEnemy();
    }

    private void DefeatEnemy()
    {
        if (currentEnemy == null)
            return;

        BeyData defeated = currentEnemy;

        currentEnemy = null;
        defeated.OnRPMDepleted -= EnemyDefeated;

        enemyUI?.Bind(null);

        Destroy(defeated.gameObject);

        if (battle == null || battle.Finished)
            return;

        enemiesDefeated++;
        ResolveBattle();

        if (battle.Finished)
            return;

        StartCoroutine(NextWaveRoutine());
    }

    private void ResetPlayerForNextWave()
    {
        if (player == null || player.Data == null)
            return;

        WaveReset.Prepare(player, Vector3.zero);
        player.Data.RestoreRPM();

        waveResetAt = Time.time;
    }

    private IEnumerator NextWaveRoutine()
    {
        yield return new WaitForSeconds(1f);

        if (battle == null || battle.Finished)
            yield break;

        if (garage == null)
            garage = GarageUI.EnsureInScene();

        if (garage != null && player != null)
            garage.Show(player, BeginWave);
        else
            BeginWave();
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
            arena != null ? arena.Radii : Vector2.zero,
            playerBurst,
            enemyBurst);

        GameResult? result = battle.Evaluate(context);

        if (!result.HasValue)
            return;

        if (battle.Reason == FinishReason.Burst)
            ServiceLocator.Get<CameraShake>()?.Shake(0.6f);

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

        if (impact.Attacker == subscribedPlayer &&
            impact.AttackerCharged &&
            impact.Clash &&
            currentEnemy != null &&
            IsChargedClashBurst(
                impact.Result.DamageToB,
                currentEnemy))
        {
            enemyBurst = true;
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

    public static bool IsChargedClashBurst(
        float damage,
        float maxRPM,
        float threshold)
    {
        return maxRPM > 0f && damage >= maxRPM * threshold;
    }

    public static bool IsChargedClashBurst(float damage, BeyData target)
    {
        if (target == null || target.Base == null || target.Base.maxRPM <= 0f)
            return false;

        CombatTuning tuning = ContentRegistry.Tuning;
        float threshold = tuning != null
            ? tuning.chargedClashBurstThreshold
            : 0.32f;

        return IsChargedClashBurst(
            damage,
            target.Base.maxRPM,
            threshold);
    }
}
