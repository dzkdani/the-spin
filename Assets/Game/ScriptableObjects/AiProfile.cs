using UnityEngine;

[CreateAssetMenu(
    fileName = "AiProfile",
    menuName = "Bey/AI Profile")]
public class AiProfile : ScriptableObject
{
    [Header("Wander")]
    public float jitterRadius = 1.2f;

    public float jitterInterval = 1f;

    [Header("Dash")]
    public float farDistance = 4f;

    public float dashChanceFar = 0.7f;

    public float dashChanceNear = 0.2f;

    public float dashIntervalMin = 1.5f;

    public float dashIntervalMax = 3f;
}
