using UnityEngine;

public sealed class RingOutCondition : IWinCondition
{
    public GameResult Result => GameResult.Lose;

    public FinishReason Reason => FinishReason.RingOut;

    public bool IsSatisfied(in BattleContext context)
    {
        return context.ArenaRadius > 0f &&
            context.PlayerPosition.magnitude > context.ArenaRadius;
    }
}
