using UnityEngine;

[CreateAssetMenu(
    fileName = "AiProfile",
    menuName = "Bey/AI Profile")]
public class AiProfile : ContentDefinition
{
    [Header("Wander")]
    public float jitterRadius = 1.2f;

    public float jitterInterval = 1f;

    [Header("Dash")]
    public float farDistance = 4f;

    public float dashIntervalMin = 1.5f;

    public float dashIntervalMax = 3f;

    [Header("Decision")]
    public float decisionInterval = 0.15f;

    public float commitTime = 0.6f;

    public float dashWindUp = 0.15f;

    [Header("Behavior Weights")]
    public float pressureWeight = 1f;

    public float elementWeight = 0.8f;

    public float baitWeight = 0.7f;

    public float strikeWeight = 1f;

    public float retreatWeight = 1.2f;

    public float activeWeight = 0.9f;

    [Header("Thresholds")]
    public float strikeRange = 4f;

    public float retreatRpm = 0.25f;

    /// <summary>
    /// Sign picks the orbit direction; magnitude scales the tangential step.
    /// </summary>
    public float retreatOrbit = 1f;

    public float dashAggression = 0.5f;
}
