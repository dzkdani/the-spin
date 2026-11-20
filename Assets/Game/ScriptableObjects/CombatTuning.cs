using UnityEngine;

public enum GrindMode
{
    Symmetric,
    SelfRpm
}

[CreateAssetMenu(
    fileName = "CombatTuning",
    menuName = "Bey/Combat Tuning")]
public class CombatTuning : ScriptableObject
{
    [Header("Impact")]
    public float minRpmPower = 0.5f;

    public float maxRpmPower = 1f;

    public float damageMultiplier = 2f;

    public float oppositeSpinMultiplier = 1.5f;

    public float chargeMultiplier = 1.75f;

    [Header("Burst")]
    public float chargedClashBurstThreshold = 0.32f;

    [Header("Spin and Dash")]
    public float sameSpinMultiplier = 1f;

    public float dashHitMultiplier = 1.25f;

    public float clashMultiplier = 1.5f;

    public float spinStealRate = 0.1f;

    [Header("Weights")]
    public float minWeightFactor = 0.5f;

    public float maxWeightFactor = 1.5f;

    [Header("Grind")]
    public GrindMode grindMode = GrindMode.Symmetric;

    public float grindRate = 0.001f;

    [Header("Effects")]
    public float shakePerSpeed = 0.02f;

    public float shakeMin = 0.05f;

    public float shakeMax = 0.35f;

    [Header("Defense")]
    public float defenseConstant = 40f;

    [Header("Limits")]
    public int maxEquippedPassives = 2;

    public int maxEquippedActives = 1;

    public int maxGrantedPassives = 2;

    public int maxGrantedActives = 1;

    public int partBudgetCap = 10;

    public int maxPendingImpactMods = 3;

    public float auraTickInterval = 0.1f;

    public float maxKnockback = 12f;
}
