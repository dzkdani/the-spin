using UnityEngine;

public sealed class BaitBehavior : IAiBehavior
{
    public string Id => "bait";

    public float Score(in BattleSnapshot snapshot, AiProfile profile)
    {
        return snapshot.OpponentDashing
            ? profile.baitWeight
            : 0f;
    }

    public AiCommand Decide(in BattleSnapshot snapshot, AiProfile profile)
    {
        Vector2 away =
            (snapshot.SelfPosition - snapshot.OpponentPosition).normalized;

        return new AiCommand(
            snapshot.SelfPosition + away * 2f,
            false,
            -1);
    }
}
