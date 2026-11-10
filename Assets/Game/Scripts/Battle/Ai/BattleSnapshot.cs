using UnityEngine;

public readonly struct BattleSnapshot
{
    public readonly float SelfRpm;
    public readonly float OpponentRpm;
    public readonly Vector2 SelfPosition;
    public readonly Vector2 OpponentPosition;
    public readonly float Distance;
    public readonly ElementDefinition SelfElement;
    public readonly ElementDefinition OpponentElement;
    public readonly bool OpponentDashing;
    public readonly float Matchup;
    public readonly float Now;

    public BattleSnapshot(
        float selfRpm,
        float opponentRpm,
        Vector2 selfPosition,
        Vector2 opponentPosition,
        float distance,
        ElementDefinition selfElement,
        ElementDefinition opponentElement,
        bool opponentDashing,
        float matchup,
        float now)
    {
        SelfRpm = selfRpm;
        OpponentRpm = opponentRpm;
        SelfPosition = selfPosition;
        OpponentPosition = opponentPosition;
        Distance = distance;
        SelfElement = selfElement;
        OpponentElement = opponentElement;
        OpponentDashing = opponentDashing;
        Matchup = matchup;
        Now = now;
    }
}
