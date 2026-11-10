using UnityEngine;

public sealed class PressureBehavior : IAiBehavior
{
    public string Id => "pressure";

    public float Score(in BattleSnapshot snapshot, AiProfile profile)
    {
        if (snapshot.Distance > 6f)
            return 0f;

        float advantage =
            snapshot.SelfRpm - snapshot.OpponentRpm;

        return Mathf.Clamp01(advantage) * profile.pressureWeight;
    }

    public AiCommand Decide(in BattleSnapshot snapshot, AiProfile profile)
    {
        return new AiCommand(snapshot.OpponentPosition, false, -1);
    }
}
