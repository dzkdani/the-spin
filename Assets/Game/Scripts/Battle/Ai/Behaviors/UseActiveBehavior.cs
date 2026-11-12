using UnityEngine;

public sealed class UseActiveBehavior : IAiBehavior
{
    public string Id => "use_active";

    public float Score(in BattleSnapshot snapshot, AiProfile profile)
    {
        if (ReadySlot(snapshot) < 0 ||
            snapshot.SelfRpm < 0.3f ||
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
            ReadySlot(snapshot));
    }

    private static int ReadySlot(in BattleSnapshot snapshot)
    {
        if (snapshot.ActiveReady == null)
            return -1;

        for (int i = 0; i < snapshot.ActiveReady.Length; i++)
        {
            if (snapshot.ActiveReady[i])
                return i;
        }

        return -1;
    }
}
