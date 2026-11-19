using UnityEngine;

public sealed class RingOutCondition : IWinCondition
{
    public GameResult Result => GameResult.Lose;

    public FinishReason Reason => FinishReason.RingOut;

    public bool IsSatisfied(in BattleContext context)
    {
        if (context.ArenaRadii.x <= 0f || context.ArenaRadii.y <= 0f)
            return false;

        return ArenaBounds.NormalizedDistance(
            context.PlayerPosition,
            Vector2.zero,
            context.ArenaRadii) > 1f;
    }
}
