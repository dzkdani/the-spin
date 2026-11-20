using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Ability",
    menuName = "Bey/Ability")]
public class AbilityDefinition : ContentDefinition
{
    public AbilityKind kind;

    public PassiveMode mode = PassiveMode.Triggered;

    public string[] tags = new string[0];

    public ElementDefinition element;

    public TriggerId trigger;

    [SerializeReference]
    public List<IAbilityCondition> conditions =
        new List<IAbilityCondition>();

    [SerializeReference]
    public List<IAbilityEffect> effects =
        new List<IAbilityEffect>();

    public ActiveSpec active = new ActiveSpec();

    [Header("Feedback")]
    public Color accent = Color.white;

    public float vfxScale = 1f;
}
