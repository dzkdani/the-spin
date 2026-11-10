using System.Collections.Generic;
using UnityEngine;

public struct ImpactResult
{
    public float DamageToA;
    public float DamageToB;

    public float RpmGainA;
    public float RpmGainB;

    public Vector2 ImpulseOnA;
    public Vector2 ImpulseOnB;

    public List<string> PendingModsUsedA;
    public List<string> PendingModsUsedB;

    public float Intensity;
}
