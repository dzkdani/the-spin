using UnityEngine;

public class BeyCombat : MonoBehaviour
{
    BeyData data;
    BeyPhysics physics;

    [SerializeField]
    int chargedHits = 0;
    
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

    public float ConsumeChargeBonus(float hitPower)
    {
        return ApplyChargeBonus(hitPower);
    }

    private float ApplyChargeBonus(float hitPower)
    {
        if (chargedHits <= 0)
            return hitPower;

        chargedHits--;

        return hitPower * ContentRegistry.Tuning.chargeMultiplier;
    }

    private void ResolveImpact(BeyController other, Collision2D collision)
    {
        if (data == null || other == null || other.Data == null)
            return;

        if (!IsCollisionOwner(other))
            return;

        CombatTuning tuning = ContentRegistry.Tuning;

        float speed =
            collision.relativeVelocity.magnitude;

        float myRPM =
            data.RPMNormalized;

        float enemyRPM =
            other.Data.RPMNormalized;

        float myAttack =
            data.Stats.Get(StatId.Attack);

        float enemyAttack =
            other.Data.Stats.Get(StatId.Attack);

        float myHitPower =
            speed *
            myAttack *
            Mathf.Lerp(tuning.minRpmPower, tuning.maxRpmPower, myRPM);

        myHitPower =
            ApplyChargeBonus(myHitPower);

        float enemyHitPower =
            speed *
            enemyAttack *
            Mathf.Lerp(tuning.minRpmPower, tuning.maxRpmPower, enemyRPM);

        enemyHitPower = other.Combat != null
            ? other.Combat.ConsumeChargeBonus(enemyHitPower)
            : enemyHitPower;

        bool oppositeSpin =
            data.Base.spinDirection !=
            other.Data.Base.spinDirection;

        if (oppositeSpin)
        {
            myHitPower *= tuning.oppositeSpinMultiplier;
            enemyHitPower *= tuning.oppositeSpinMultiplier;
        }

        float myWeight =
            GetEffectiveWeight(data, tuning);

        float enemyWeight =
            GetEffectiveWeight(other.Data, tuning);

        Vector2 pushDir =
            (other.transform.position -
            transform.position).normalized;

        // Knockback

        other.Physics.Rigidbody.AddForce(
            pushDir *
            (myHitPower / enemyWeight),
            ForceMode2D.Impulse);

        physics.Rigidbody.AddForce(
            -pushDir *
            (enemyHitPower / myWeight),
            ForceMode2D.Impulse);

        // RPM Damage

        float damageToEnemy =
            myHitPower * tuning.damageMultiplier;

        float damageToMe =
            enemyHitPower * tuning.damageMultiplier;

        other.Data.ReduceRPM(
            damageToEnemy);

        data.ReduceRPM(
            damageToMe);

        ResolveEffect(speed, collision);
    }

    private void ResolveEffect(float speed, Collision2D collision)
    {
        CombatTuning tuning = ContentRegistry.Tuning;

        ServiceLocator.Get<SFXPlayer>()?.PlayHit();

        ServiceLocator.Get<CameraShake>()?.Shake(
            Mathf.Clamp(
                speed * tuning.shakePerSpeed,
                tuning.shakeMin,
                tuning.shakeMax));

        if (collision.contactCount == 0)
            return;

        Vector3 point =
            collision.GetContact(0).point;

        ServiceLocator.Get<HitEffectSpawner>()?.Spawn(point);
    }

    private void ResolveGrind(BeyController other, Collision2D collision)
    {
        if (data == null || other == null || other.Data == null)
            return;

        if (!IsCollisionOwner(other))
            return;

        float rpmDifference =
            Mathf.Abs(
                data.CurrentRPM -
                other.Data.CurrentRPM);

        float grindDamage =
            rpmDifference *
            ContentRegistry.Tuning.grindRate *
            Time.fixedDeltaTime;

        data.ReduceRPM(
            grindDamage);

        other.Data.ReduceRPM(
            grindDamage);
    }

    private float GetEffectiveWeight(BeyData target, CombatTuning tuning)
    {
        return target.Stats.Get(StatId.Weight) *
            Mathf.Lerp(
                tuning.minWeightFactor,
                tuning.maxWeightFactor,
                target.RPMNormalized);
    }
}
