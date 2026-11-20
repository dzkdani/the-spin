using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class BeyPhysics : MonoBehaviour
{
    Rigidbody2D rb;
    public Rigidbody2D Rigidbody => rb;
    BeyData data;
    IBeyInput input;

    public event Action<BeyController, Collision2D> OnImpact;
    public event Action<BeyController, Collision2D> OnGrind;

    private float dashTimer;

    public bool IsDashing => dashTimer > 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(BeyData data, IBeyInput input)
    {
        this.data = data;
        this.input = input;

        data.OnRPMDepleted -= Stop;
        data.OnRPMDepleted += Stop;
    }

    private void OnDestroy()
    {
        if (data != null)
            data.OnRPMDepleted -= Stop;
    }

    private void FixedUpdate()
    {
        if (data == null || input == null || data.Base == null)
            return;

        if (dashTimer > 0)
        {
            dashTimer -= Time.fixedDeltaTime;

            if (dashTimer <= 0f)
                BattleEvents.RaiseDashEnded(data);

            return;
        }

        Move();
    }

    private void Stop()
    {
        rb.linearVelocity = Vector2.zero;
    }

    private void Move()
    {
        Vector2 delta =
            input.MoveTarget - rb.position;

        if (delta.sqrMagnitude < 0.01f)
            return;

        Vector2 direction =
            delta.normalized;

        float moveFactor =
            Mathf.Lerp
            (
                data.Base.minMoveAccelerationMultiplier,
                1f,
                data.RPMNormalized
            );

        float currentAcceleration =
            data.Base.acceleration * moveFactor;

        float currentMaxVelocity =
            data.Base.maxVelocity;

        rb.AddForce(direction * currentAcceleration, ForceMode2D.Force);

        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, currentMaxVelocity);
    }

    public bool Dash(Vector2 direction)
    {
        return Dash(direction, 1f);
    }

    public bool Dash(Vector2 direction, float power)
    {
        if (data.CurrentRPM < data.Base.dashRPMCost)
            return false;

        rb.AddForce(
            direction.normalized * data.Base.dashImpulse * power,
            ForceMode2D.Impulse);

        dashTimer = 0.2f;

        data.ReduceRPM(data.Base.dashRPMCost);

        BattleEvents.RaiseDashStarted(data);

        return true;
    }

    public void Launch(Vector2 direction, float impulse)
    {
        if (rb == null)
            return;

        rb.linearVelocity = Vector2.zero;

        if (direction.sqrMagnitude < 0.0001f || impulse <= 0f)
            return;

        rb.AddForce(
            direction.normalized * impulse,
            ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        BeyController other =
            collision.gameObject.GetComponent<BeyController>();

        if (other == null)
            return;

        OnImpact?.Invoke(other, collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        BeyController other =
            collision.gameObject.GetComponent<BeyController>();

        if (other == null)
            return;

        OnGrind?.Invoke(other, collision);
    }
}
