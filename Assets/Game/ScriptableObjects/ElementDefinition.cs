using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Element",
    menuName = "Bey/Element")]
public class ElementDefinition : ContentDefinition
{
    [Header("Identity")]
    public bool isNeutral;

    public Color color = Color.white;

    [Header("On Hit")]
    public List<StatModifierSpec> onHitModifiers =
        new List<StatModifierSpec>();

    public bool onHitAffectsSelf;

    public float onHitDuration = 2f;
}
