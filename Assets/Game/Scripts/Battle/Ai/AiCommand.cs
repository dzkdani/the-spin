using UnityEngine;

public readonly struct AiCommand
{
    public readonly Vector2 MoveTarget;
    public readonly bool RequestDash;
    public readonly int AbilitySlot;

    public AiCommand(
        Vector2 moveTarget,
        bool requestDash,
        int abilitySlot)
    {
        MoveTarget = moveTarget;
        RequestDash = requestDash;
        AbilitySlot = abilitySlot;
    }
}
