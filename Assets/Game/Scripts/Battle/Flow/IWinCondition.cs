public interface IWinCondition
{
    bool IsSatisfied(in BattleContext context);

    GameResult Result { get; }
}
