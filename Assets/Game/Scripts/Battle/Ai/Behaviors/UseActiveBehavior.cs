using UnityEngine;

public sealed class UseActiveBehavior : IAiBehavior
{
    public string Id => "use_active";

    public float Score(in BattleSnapshot snapshot, AiProfile profile)
    {
        if (snapshot.SelfRpm < 0.3f ||
            snapshot.OpponentRpm > 0.45f)
        {
            return 0f;
        }

        float urgency = 1f - snapshot.OpponentRpm / 0.45f;

        return profile.activeWeight *
            Mathf.Lerp(0.5f, 1f, Mathf.Clamp01(urgency));
    }

    public AiCommand Decide(in BattleSnapshot snapshot, AiProfile profile)
    {
        return new AiCommand(
            snapshot.OpponentPosition,
            false,
            0);
    }
}
