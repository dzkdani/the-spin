public readonly struct BattleContext
{
    public readonly float PlayerRpm;
    public readonly int EnemiesDefeated;
    public readonly int TotalEnemies;

    public BattleContext(
        float playerRpm,
        int enemiesDefeated,
        int totalEnemies)
    {
        PlayerRpm = playerRpm;
        EnemiesDefeated = enemiesDefeated;
        TotalEnemies = totalEnemies;
    }
}
