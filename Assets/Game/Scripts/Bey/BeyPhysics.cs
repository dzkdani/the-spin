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

    private void FixedUpdate()
    {
        if (dashTimer > 0)
        {
            dashTimer -= Time.fixedDeltaTime;
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

        float rpmFactor =
            data.RPMNormalized;

        Vector2 direction =
            delta.normalized;

        float currentAcceleration =
            data.Base.acceleration * rpmFactor;

        float currentMaxVelocity =
            data.Base.maxVelocity * rpmFactor;

        rb.AddForce(direction * currentAcceleration, ForceMode2D.Force);

        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, currentMaxVelocity);
    }

    public void Dash(Vector2 direction)
    {
        if (data.CurrentRPM < data.Base.dashRPMCost)
            return;

        float rpmFactor = 
            Mathf.Clamp(data.RPMNormalized, 0.3f, 1f);

        rb.linearVelocity +=
            data.Base.dashImpulse * rpmFactor * direction.normalized;
        
        dashTimer = 0.2f;

        data.ReduceRPM(data.Base.dashRPMCost);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        BeyController other =
            collision.gameObject.GetComponent<BeyController>();

        if (other == null)
            return;

        OnImpact?.Invoke(other, collision);
        Debug.Log($"{gameObject.name} collided with {other.gameObject.name}");
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
