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

        return hitPower * 1.75f;
    }

    private void ResolveImpact(BeyController other, Collision2D collision)
    {
        if (data == null || other == null || other.Data == null)
            return;

        if (!IsCollisionOwner(other))
            return;

        float speed =
            collision.relativeVelocity.magnitude;

        float myRPM =
            data.RPMNormalized;

        float enemyRPM =
            other.Data.RPMNormalized;

        float myAttack =
            data.Base.attack;

        float enemyAttack =
            other.Data.Base.attack;

        float myHitPower =
            speed *
            myAttack *
            Mathf.Lerp(0.5f, 1f, myRPM);

        myHitPower =
            ApplyChargeBonus(myHitPower);

        float enemyHitPower =
            speed *
            enemyAttack *
            Mathf.Lerp(0.5f, 1f, enemyRPM);

        enemyHitPower = other.Combat != null
            ? other.Combat.ConsumeChargeBonus(enemyHitPower)
            : enemyHitPower;

        bool oppositeSpin =
            data.Base.spinDirection !=
            other.Data.Base.spinDirection;

        if (oppositeSpin)
        {
            myHitPower *= 1.5f;
            enemyHitPower *= 1.5f;
        }

        float myWeight =
            GetEffectiveWeight(data);

        float enemyWeight =
            GetEffectiveWeight(other.Data);

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
            myHitPower * 2f;

        float damageToMe =
            enemyHitPower * 2f;

        other.Data.ReduceRPM(
            damageToEnemy);

        data.ReduceRPM(
            damageToMe);

        ResolveEffect(speed, collision);
    }

    private void ResolveEffect(float speed, Collision2D collision)
    {
        ServiceLocator.Get<SFXPlayer>()?.PlayHit();

        ServiceLocator.Get<CameraShake>()?.Shake(
            Mathf.Clamp(speed * 0.02f, 0.05f, 0.35f));

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
            0.001f *
            Time.fixedDeltaTime;

        data.ReduceRPM(
            grindDamage);

        other.Data.ReduceRPM(
            grindDamage);
    }

    private float GetEffectiveWeight(BeyData target)
    {
        return target.Base.weight *
            Mathf.Lerp(
                0.5f,
                1.5f,
                target.RPMNormalized);
    }
}
