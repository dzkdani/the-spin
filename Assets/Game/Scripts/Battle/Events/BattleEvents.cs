using System;
using UnityEngine;

public static class BattleEvents
{
    public static event Action<ImpactEvent> ImpactPending;
    public static event Action<ImpactEvent> ImpactResolved;
    public static event Action<BeyData> DashStarted;
    public static event Action<BeyData> DashEnded;
    public static event Action<BeyData, float> RpmChanged;
    public static event Action<BeyData, string> AbilityActivated;
    public static event Action<BeyData, string> CastComplete;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Reset()
    {
        ImpactPending = null;
        ImpactResolved = null;
        DashStarted = null;
        DashEnded = null;
        RpmChanged = null;
        AbilityActivated = null;
        CastComplete = null;
    }

    public static void RaiseImpactPending(ImpactEvent impact)
    {
        ImpactPending?.Invoke(impact);
    }

    public static void RaiseImpactResolved(ImpactEvent impact)
    {
        ImpactResolved?.Invoke(impact);
    }

    public static void RaiseDashStarted(BeyData bey)
    {
        DashStarted?.Invoke(bey);
    }

    public static void RaiseDashEnded(BeyData bey)
    {
        DashEnded?.Invoke(bey);
    }

    public static void RaiseRpmChanged(BeyData bey, float rpm)
    {
        RpmChanged?.Invoke(bey, rpm);
    }

    public static void RaiseAbilityActivated(BeyData bey, string abilityId)
    {
        AbilityActivated?.Invoke(bey, abilityId);
    }

    public static void RaiseCastComplete(BeyData bey, string abilityId)
    {
        CastComplete?.Invoke(bey, abilityId);
    }
}
