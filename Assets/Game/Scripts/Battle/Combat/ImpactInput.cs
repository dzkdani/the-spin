using System.Collections.Generic;
using UnityEngine;

public sealed class ImpactInput
{
    public float Speed;
    public float RpmA;
    public float RpmB;

    public IStatView StatsA;
    public IStatView StatsB;

    public ElementDefinition ElementA;
    public ElementDefinition ElementB;
    public MatchupTable Matchups;

    public bool OppositeSpin;
    public Vector2 Normal;

    public List<PendingImpactMod> PendingA = new List<PendingImpactMod>();
    public List<PendingImpactMod> PendingB = new List<PendingImpactMod>();

    public float Now;
    public CombatTuning Tuning;
}
