using System;
using UnityEngine;

public static class ImpactSolver
{
    private static readonly IImpactRule[] rules =
    {
        new BaseHitPowerRule(),
        new MatchupRule(),
        new SpinRule(),
        new SameSpinRule(),
        new SpinStealRule(),
        new PendingImpactRule(),
        new ClashRule(),
        new DashHitRule()
    };

    static ImpactSolver()
    {
        Array.Sort(rules, (a, b) => a.Order.CompareTo(b.Order));
    }

    public static ImpactResult Resolve(ImpactInput input)
    {
        if (input.Tuning == null)
            input.Tuning = ContentRegistry.Tuning;

        ImpactState state = new ImpactState();

        for (int i = 0; i < rules.Length; i++)
            rules[i].Apply(state, input);

        CombatTuning tuning = input.Tuning;

        float damageToA = Mitigate(
            state.PowerB * tuning.damageMultiplier,
            input.StatsA.Get(StatId.Defense),
            tuning.defenseConstant);

        float damageToB = Mitigate(
            state.PowerA * tuning.damageMultiplier,
            input.StatsB.Get(StatId.Defense),
            tuning.defenseConstant);

        float weightA = EffectiveWeight(
            input.StatsA,
            input.RpmA,
            tuning);

        float weightB = EffectiveWeight(
            input.StatsB,
            input.RpmB,
            tuning);

        Vector2 impulseOnA =
            -input.Normal *
            ClampKnockback(state.PowerB / weightA, tuning);

        Vector2 impulseOnB =
            input.Normal *
            ClampKnockback(state.PowerA / weightB, tuning);

        return new ImpactResult
        {
            DamageToA = damageToA,
            DamageToB = damageToB,
            RpmGainA = state.StealToA,
            RpmGainB = state.StealToB,
            ImpulseOnA = impulseOnA,
            ImpulseOnB = impulseOnB,
            PendingModsUsedA = state.PendingUsedA,
            PendingModsUsedB = state.PendingUsedB,
            Intensity = Mathf.Clamp01(input.Speed * tuning.shakePerSpeed)
        };
    }

    private static float Mitigate(float damage, float defense, float k)
    {
        float resolvedDefense = Mathf.Max(0f, defense);

        return damage * k / (k + resolvedDefense);
    }

    private static float EffectiveWeight(
        IStatView stats,
        float rpmNormalized,
        CombatTuning tuning)
    {
        float tipFriction = stats.Get(StatId.TipFriction);

        if (tipFriction <= 0f)
            tipFriction = 1f;

        float weight =
            stats.Get(StatId.Weight) *
            tipFriction *
            Mathf.Lerp(
                tuning.minWeightFactor,
                tuning.maxWeightFactor,
                rpmNormalized);

        return Mathf.Max(0.0001f, weight);
    }

    private static float ClampKnockback(float impulse, CombatTuning tuning)
    {
        return Mathf.Min(impulse, tuning.maxKnockback);
    }
}
