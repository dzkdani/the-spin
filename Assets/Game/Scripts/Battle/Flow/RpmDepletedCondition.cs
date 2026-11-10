public sealed class RpmDepletedCondition : IWinCondition
{
    public GameResult Result => GameResult.Lose;

    public bool IsSatisfied(in BattleContext context)
    {
        return context.PlayerRpm <= 0f;
    }
}
