using UnityEngine;

public sealed class RetreatBehavior : IAiBehavior
{
    private const float OrbitSpeed = 3f;
    private const float EdgeMargin = 0.8f;

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
        Vector2 toCenter = snapshot.ArenaCenter - snapshot.SelfPosition;
        float radius = toCenter.magnitude;
        Vector2 inward = radius > 0.0001f
            ? toCenter / radius
            : Vector2.up;

        Vector2 tangent = new Vector2(-inward.y, inward.x);

        if (profile.retreatOrbit < 0f)
            tangent = -tangent;

        float orbitStep = OrbitSpeed * Mathf.Abs(profile.retreatOrbit);
        float edgeBias = 0f;

        if (snapshot.ArenaRadius > 0f)
        {
            float limit = snapshot.ArenaRadius - EdgeMargin;
            if (radius > limit)
                edgeBias = Mathf.Clamp01((radius - limit) / EdgeMargin) * 2f;
        }

        Vector2 target = snapshot.SelfPosition +
            tangent * orbitStep +
            inward * (OrbitSpeed * edgeBias);

        if (snapshot.ArenaRadius > 0f)
        {
            float limit = snapshot.ArenaRadius - EdgeMargin;
            Vector2 offset = target - snapshot.ArenaCenter;

            if (offset.sqrMagnitude > limit * limit)
                target = snapshot.ArenaCenter + offset.normalized * limit;
        }

        return new AiCommand(target, false, -1);
    }
}
