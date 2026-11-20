using UnityEngine;

public readonly struct ImpactEvent
{
    public readonly BeyData Attacker;
    public readonly BeyData Defender;
    public readonly ImpactResult Result;
    public readonly Vector2 Point;
    public readonly float Shake;
    public readonly float Speed;
    public readonly bool OppositeSpin;
    public readonly bool AttackerCharged;
    public readonly bool Clash;

    public ImpactEvent(
        BeyData attacker,
        BeyData defender,
        ImpactResult result,
        Vector2 point,
        float shake,
        float speed,
        bool oppositeSpin,
        bool attackerCharged = false,
        bool clash = false)
    {
        Attacker = attacker;
        Defender = defender;
        Result = result;
        Point = point;
        Shake = shake;
        Speed = speed;
        OppositeSpin = oppositeSpin;
        AttackerCharged = attackerCharged;
        Clash = clash;
    }
}
