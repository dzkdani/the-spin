using UnityEngine;

[CreateAssetMenu(
    fileName = "BeyBase",
    menuName = "Bey/Bey Base")]
public class BeyBaseSO : ScriptableObject
{
    [Header("Stats")]
    public float maxVelocity = 15f;
    public float dashStrength = 50f;
    public float maxRPM = 3000f;
    public float acceleration = 25f;
    public float dashRPMCost = 50f;
    public float dashImpulse = 10f;
    public float attack = 10f;
    public float weight = 5f;
    public SpinDirection spinDirection = SpinDirection.Clockwise;
}

public enum SpinDirection
{
    Clockwise,
    CounterClockwise
}