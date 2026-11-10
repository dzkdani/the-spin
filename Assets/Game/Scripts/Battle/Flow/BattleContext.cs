using UnityEngine;

public readonly struct BattleContext
{
    public readonly float PlayerRpm;
    public readonly int EnemiesDefeated;
    public readonly int TotalEnemies;
    public readonly Vector2 PlayerPosition;
    public readonly float ArenaRadius;
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
            0f,
            false,
            false)
    {
    }

    public BattleContext(
        float playerRpm,
        int enemiesDefeated,
        int totalEnemies,
        Vector2 playerPosition,
        float arenaRadius)
        : this(
            playerRpm,
            enemiesDefeated,
            totalEnemies,
            playerPosition,
            arenaRadius,
            false,
            false)
    {
    }

    public BattleContext(
        float playerRpm,
        int enemiesDefeated,
        int totalEnemies,
        Vector2 playerPosition,
        float arenaRadius,
        bool playerBurst,
        bool enemyBurst)
    {
        PlayerRpm = playerRpm;
        EnemiesDefeated = enemiesDefeated;
        TotalEnemies = totalEnemies;
        PlayerPosition = playerPosition;
        ArenaRadius = arenaRadius;
        PlayerBurst = playerBurst;
        EnemyBurst = enemyBurst;
    }
}
