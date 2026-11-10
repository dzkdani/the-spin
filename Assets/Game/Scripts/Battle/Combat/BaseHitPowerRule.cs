using UnityEngine;

public sealed class BaseHitPowerRule : IImpactRule
{
    public int Order => 100;

    public void Apply(ImpactState state, ImpactInput input)
    {
        CombatTuning tuning = input.Tuning;

        state.PowerA =
            input.Speed *
            input.StatsA.Get(StatId.Attack) *
            Mathf.Lerp(tuning.minRpmPower, tuning.maxRpmPower, input.RpmA);

        state.PowerB =
            input.Speed *
            input.StatsB.Get(StatId.Attack) *
            Mathf.Lerp(tuning.minRpmPower, tuning.maxRpmPower, input.RpmB);
    }
}
