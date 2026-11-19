using UnityEngine;

public sealed class RetreatBehavior : IAiBehavior
{
    private const float OrbitSpeed = 3f;
    private const float EdgeMargin = 0.8f;
    private const float Cooldown = 2f;

    private float nextAllowedAt = float.NegativeInfinity;

    public string Id => "retreat";

    public float Score(in BattleSnapshot snapshot, AiProfile profile)
    {
        if (snapshot.Now < nextAllowedAt)
            return 0f;

        if (snapshot.SelfRpm >= profile.retreatRpm)
            return 0f;

        float urgency =
            1f - snapshot.SelfRpm / Mathf.Max(0.0001f, profile.retreatRpm);

        return Mathf.Clamp01(urgency) * profile.retreatWeight;
    }

    public AiCommand Decide(in BattleSnapshot snapshot, AiProfile profile)
    {
        float urgency = Mathf.Clamp01(
            1f - snapshot.SelfRpm / Mathf.Max(0.0001f, profile.retreatRpm));

        nextAllowedAt = snapshot.Now + Cooldown;

        Vector2 toCenter = snapshot.ArenaCenter - snapshot.SelfPosition;
        float radius = toCenter.magnitude;
        Vector2 inward = radius > 0.0001f
            ? toCenter / radius
            : Vector2.up;

        Vector2 tangent = new Vector2(-inward.y, inward.x);

        float orbit = profile.retreatOrbit;

        if (Mathf.Approximately(orbit, 0f))
            orbit = 1f;

        if (orbit < 0f)
            tangent = -tangent;

        float tangentStep = Mathf.Lerp(
            OrbitSpeed * 0.8f,
            OrbitSpeed * 0.4f,
            urgency) * Mathf.Abs(orbit);

        float inwardStep = Mathf.Lerp(0f, OrbitSpeed * 0.5f, urgency);

        if (snapshot.ArenaRadius > 0f)
        {
            float limit = snapshot.ArenaRadius - EdgeMargin;

            if (radius > limit)
            {
                inwardStep +=
                    Mathf.Clamp01((radius - limit) / EdgeMargin) *
                    OrbitSpeed * 2f;
            }
        }

        Vector2 target = snapshot.SelfPosition +
            tangent * tangentStep +
            inward * inwardStep;

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
