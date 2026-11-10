public sealed class EnemiesClearedCondition : IWinCondition
{
    public GameResult Result => GameResult.Win;

    public bool IsSatisfied(in BattleContext context)
    {
        return context.TotalEnemies > 0 &&
            context.EnemiesDefeated >= context.TotalEnemies;
    }
}
