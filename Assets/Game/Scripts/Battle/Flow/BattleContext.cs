using UnityEngine;

public readonly struct BattleContext
{
    public readonly float PlayerRpm;
    public readonly int EnemiesDefeated;
    public readonly int TotalEnemies;
    public readonly Vector2 PlayerPosition;
    public readonly Vector2 ArenaRadii;
    public readonly bool PlayerBurst;
    public readonly bool EnemyBurst;

    public BattleContext(
        float playerRpm,
        int enemiesDefeated,
        int totalEnemies)
        : this(
            playerRpm,
            enemiesDefeated,
            totalEnemies,
            Vector2.zero,
            Vector2.zero,
            false,
            false)
    {
    }

    public BattleContext(
        float playerRpm,
        int enemiesDefeated,
        int totalEnemies,
        Vector2 playerPosition,
        Vector2 arenaRadii)
        : this(
            playerRpm,
            enemiesDefeated,
            totalEnemies,
            playerPosition,
            arenaRadii,
            false,
            false)
    {
    }

    public BattleContext(
        float playerRpm,
        int enemiesDefeated,
        int totalEnemies,
        Vector2 playerPosition,
        Vector2 arenaRadii,
        bool playerBurst,
        bool enemyBurst)
    {
        PlayerRpm = playerRpm;
        EnemiesDefeated = enemiesDefeated;
        TotalEnemies = totalEnemies;
        PlayerPosition = playerPosition;
        ArenaRadii = arenaRadii;
        PlayerBurst = playerBurst;
        EnemyBurst = enemyBurst;
    }
}
