using UnityEngine;

public sealed class RetreatBehavior : IAiBehavior
{
    public string Id => "retreat";

    public float Score(in BattleSnapshot snapshot, AiProfile profile)
    {
        if (snapshot.SelfRpm >= profile.retreatRpm)
            return 0f;

        float urgency =
            1f - snapshot.SelfRpm / Mathf.Max(0.0001f, profile.retreatRpm);

        return Mathf.Clamp01(urgency) * profile.retreatWeight;
    }

    public AiCommand Decide(in BattleSnapshot snapshot, AiProfile profile)
    {
        Vector2 away =
            (snapshot.SelfPosition - snapshot.OpponentPosition).normalized;

        if (away.sqrMagnitude < 0.0001f)
            away = Vector2.right;

        return new AiCommand(
            snapshot.SelfPosition + away * 3f,
            false,
            -1);
    }
}
