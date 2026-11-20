using UnityEngine;

[CreateAssetMenu(
    fileName = "LaunchTuning",
    menuName = "Bey/Launch Tuning")]
public class LaunchTuning : ScriptableObject
{
    public bool enabled = true;

    [Header("Player")]
    public float playerRotationDegrees = 360f;

    public float minStartFactor = 0.7f;

    public float perfectThreshold = 0.9f;

    public int perfectChargedHits = 1;

    [Header("Enemy")]
    public float enemyPower = 0.9f;

    public float enemyPowerRamp = 0.01f;

    public float enemyVariance = 0.2f;

    [Header("Motion")]
    public float launchImpulse = 3f;

    public float launchSpotFactor = 0.75f;
}
