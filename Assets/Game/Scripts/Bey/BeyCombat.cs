using UnityEngine;

public class BeyCombat : MonoBehaviour
{
    BeyData data;
    BeyPhysics physics;

    [SerializeField]
    int chargedHits = 0;

    public bool HasCharge => chargedHits > 0;

    private bool IsCollisionOwner(BeyController other)
    {
        return data.GetInstanceID() <
            other.Data.GetInstanceID();
    }

    public void Initialize(BeyData data, BeyPhysics physics)
    {
        this.data = data;
        this.physics = physics;

        physics.OnImpact += ResolveImpact;
        physics.OnGrind += ResolveGrind;
    }

    private void OnDestroy()
    {
        if (physics == null)
            return;

        physics.OnImpact -= ResolveImpact;
        physics.OnGrind -= ResolveGrind;
    }

    public void AddChargedHit(int amount = 1)
    {
        chargedHits += amount;
    }

    public void ConsumeCharge()
    {
        if (chargedHits > 0)
            chargedHits--;
    }

    private void ResolveImpact(BeyController other, Collision2D collision)
    {
        if (data == null || other == null || other.Data == null)
            return;

        if (!IsCollisionOwner(other))
            return;

        CombatTuning tuning = ContentRegistry.Tuning;

        ImpactInput input = new ImpactInput
        {
            Speed = collision.relativeVelocity.magnitude,
            RpmA = data.RPMNormalized,
            RpmB = other.Data.RPMNormalized,
            StatsA = data.Stats,
            StatsB = other.Data.Stats,
            ElementA = data.Elements.Current,
            ElementB = other.Data.Elements.Current,
            Matchups = ContentRegistry.Matchups,
            OppositeSpin =
                data.Base.spinDirection !=
                other.Data.Base.spinDirection,
            Normal = GetNormal(other, collision),
            Now = Time.time,
            Tuning = tuning
        };

        if (chargedHits > 0)
            input.PendingA.Add(CreateChargeMod(tuning));

        if (other.Combat != null && other.Combat.HasCharge)
            input.PendingB.Add(CreateChargeMod(tuning));

        ImpactResult result = ImpactSolver.Resolve(input);

        Vector2 point = GetPoint(collision);

        float shake = Mathf.Lerp(
            tuning.shakeMin,
            tuning.shakeMax,
            result.Intensity);

        BattleEvents.RaiseImpactPending(
            new ImpactEvent(data, other.Data, result, point, shake));

        data.ReduceRPM(result.DamageToA);
        other.Data.ReduceRPM(result.DamageToB);

        physics.Rigidbody.AddForce(
            result.ImpulseOnA,
            ForceMode2D.Impulse);

        other.Physics.Rigidbody.AddForce(
            result.ImpulseOnB,
            ForceMode2D.Impulse);

        if (result.PendingModsUsedA.Count > 0)
            ConsumeCharge();

        if (result.PendingModsUsedB.Count > 0 &&
            other.Combat != null)
        {
            other.Combat.ConsumeCharge();
        }

        BattleEvents.RaiseImpactResolved(
            new ImpactEvent(data, other.Data, result, point, shake));
    }

    private static PendingImpactMod CreateChargeMod(CombatTuning tuning)
    {
        return new PendingImpactMod(
            "charge",
            tuning.chargeMultiplier,
            1,
            float.PositiveInfinity);
    }

    private Vector2 GetNormal(BeyController other, Collision2D collision)
    {
        if (collision.contactCount > 0)
            return -collision.GetContact(0).normal;

        Vector2 delta =
            other.transform.position -
            transform.position;

        return delta.sqrMagnitude > 0.0001f
            ? delta.normalized
            : Vector2.right;
    }

    private Vector2 GetPoint(Collision2D collision)
    {
        return collision.contactCount > 0
            ? (Vector2)collision.GetContact(0).point
            : (Vector2)transform.position;
    }

    private void ResolveGrind(BeyController other, Collision2D collision)
    {
        if (data == null || other == null || other.Data == null)
            return;

        if (!IsCollisionOwner(other))
            return;

        CombatTuning tuning = ContentRegistry.Tuning;

        GrindSolver.Result result = GrindSolver.Resolve(
            data.CurrentRPM,
            other.Data.CurrentRPM,
            data.Base.maxRPM,
            other.Data.Base.maxRPM,
            data.RPMNormalized,
            other.Data.RPMNormalized,
            tuning.grindMode,
            tuning.grindRate,
            Time.fixedDeltaTime);

        data.ReduceRPM(result.DamageA);
        other.Data.ReduceRPM(result.DamageB);
    }
}
