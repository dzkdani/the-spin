using UnityEngine;

[CreateAssetMenu(
    fileName = "BeyStats",
    menuName = "Bey/Bey Stats")]
public class BeyStats : ScriptableObject
{
    [Header("Movement")]
    public float maxVelocity = 15f;

    public float acceleration = 25f;

    public float dashRPMCost = 50f;

    public float dashImpulse = 10f;

    public float minMoveAccelerationMultiplier = 0.6f;

    [Header("Combat")]
    public float maxRPM = 3000f;

    public float attack = 10f;

    public float weight = 5f;

    public SpinDirection spinDirection = SpinDirection.Clockwise;
}

public enum SpinDirection
{
    Clockwise,
    CounterClockwise
}
