using UnityEngine;

[CreateAssetMenu(
    fileName = "BeyBase",
    menuName = "Bey/Bey Base")]
public class BeyBaseSO : ScriptableObject
{
    [Header("Stats")]
    public float maxRPM = 3000f;
    public float currentRPM = 3000f;
    public float moveSpeed = 10f;

    [Header("Combat")]
    public float attack = 10f;
    public float defense = 5f;
    public float weight = 5f;
}