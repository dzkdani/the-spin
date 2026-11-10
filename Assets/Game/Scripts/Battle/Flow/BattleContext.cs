using UnityEngine;

public readonly struct BattleContext
{
    public readonly float PlayerRpm;
    public readonly int EnemiesDefeated;
    public readonly int TotalEnemies;
    public readonly Vector2 PlayerPosition;
    public readonly float ArenaRadius;

    public BattleContext(
        float playerRpm,
        int enemiesDefeated,
        int totalEnemies)
        : this(
            playerRpm,
            enemiesDefeated,
            totalEnemies,
            Vector2.zero,
            0f)
    {
    }

    public BattleContext(
        float playerRpm,
        int enemiesDefeated,
        int totalEnemies,
        Vector2 playerPosition,
        float arenaRadius)
    {
        PlayerRpm = playerRpm;
        EnemiesDefeated = enemiesDefeated;
        TotalEnemies = totalEnemies;
        PlayerPosition = playerPosition;
        ArenaRadius = arenaRadius;
    }
}
