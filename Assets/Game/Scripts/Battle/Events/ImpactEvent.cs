using UnityEngine;

public readonly struct ImpactEvent
{
    public readonly BeyData Attacker;
    public readonly BeyData Defender;
    public readonly ImpactResult Result;
    public readonly Vector2 Point;
    public readonly float Shake;

    public ImpactEvent(
        BeyData attacker,
        BeyData defender,
        ImpactResult result,
        Vector2 point,
        float shake)
    {
        Attacker = attacker;
        Defender = defender;
        Result = result;
        Point = point;
        Shake = shake;
    }
}
