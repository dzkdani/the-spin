using UnityEngine;

[CreateAssetMenu(
    fileName = "Element",
    menuName = "Bey/Element")]
public class ElementDefinition : ContentDefinition
{
    [Header("Identity")]
    public bool isNeutral;

    public Color color = Color.white;
}
