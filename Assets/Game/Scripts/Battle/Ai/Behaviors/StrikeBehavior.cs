using UnityEngine;

public sealed class StrikeBehavior : IAiBehavior
{
    public string Id => "strike";

    public float Score(in BattleSnapshot snapshot, AiProfile profile)
    {
        if (snapshot.OpponentDashing)
            return 0f;

        if (snapshot.Distance > profile.strikeRange)
            return 0f;

        return profile.strikeWeight *
            Mathf.Max(0.1f, profile.dashAggression);
    }

    public AiCommand Decide(in BattleSnapshot snapshot, AiProfile profile)
    {
        return new AiCommand(snapshot.OpponentPosition, true, -1);
    }
}
