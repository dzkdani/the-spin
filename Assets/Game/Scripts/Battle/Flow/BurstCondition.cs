public sealed class BurstCondition : IWinCondition
{
    private readonly bool player;

    public BurstCondition(bool player)
    {
        this.player = player;
    }

    public bool IsSatisfied(in BattleContext context)
    {
        return player
            ? context.PlayerBurst
            : context.EnemyBurst;
    }

    public GameResult Result =>
        player ? GameResult.Lose : GameResult.Win;

    public FinishReason Reason => FinishReason.Burst;
}
