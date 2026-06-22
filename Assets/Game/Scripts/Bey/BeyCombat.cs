using UnityEngine;
using TMPro;

public class BeyCombat : MonoBehaviour
{
    BeyData data;
    BeyPhysics physics;

    [SerializeField]
    bool opposite;
    [SerializeField]
    int chargedHits = 0;
    
    private bool IsCollisionOwner(BeyController other)
    {
        return GetInstanceID() <
            other.GetInstanceID();
    }

    public void Initialize(BeyData data, BeyPhysics physics)
    {
        this.data = data;
        this.physics = physics;

        data.OnRPMDepleted += LoseHandler;

        physics.OnImpact += ResolveImpact;
        physics.OnGrind += ResolveGrind;
    }

    public void AddChargedHit(int amount = 1)
    {
        chargedHits += amount;
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

        Debug.Log($"{gameObject.name} hit {other.gameObject.name} for {damageToEnemy} RPM damage");
    }

    private void ResolveGrind(BeyController other, Collision2D collision)
    {
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

        Debug.Log($"{gameObject.name} is grinding against {other.gameObject.name}, losing {grindDamage} RPM");
    }

    private float GetEffectiveWeight(BeyData target)
    {
        return target.Base.weight *
            Mathf.Lerp(
                0.5f,
                1.5f,
                target.RPMNormalized);
    }


    [SerializeField]
    TMP_Text debugText;
    private void LoseHandler()
    {
        Debug.Log($"{gameObject.name} is defeated!");
        debugText.text = $"{gameObject.name} is defeated!";
    }
}
