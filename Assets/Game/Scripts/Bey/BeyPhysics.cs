using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class BeyPhysics : MonoBehaviour
{
    Rigidbody2D rb;
    BeyData data;
    IBeyInput input;

    public event Action<float> OnImpact;

    [SerializeField]
    [Tooltip("How quickly the Bey follows the input target. Higher values will be more responsive but less smooth.")]
    private float followStrength = 10f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(BeyData data, IBeyInput input)
    {
        this.data = data;
        this.input = input;
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Vector2 delta =
            input.MoveTarget - rb.position;

        Vector2 desiredVelocity =
            delta * followStrength;

        desiredVelocity =
            Vector2.ClampMagnitude(
                desiredVelocity,
                data.Base.moveSpeed);

        rb.linearVelocity = desiredVelocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        float impact =
            collision.relativeVelocity.magnitude;
        OnImpact?.Invoke(impact);
    }
}
