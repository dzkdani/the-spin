using UnityEngine;

public interface IBeyInput
{
    Vector2 MoveTarget { get; }

    bool DashRequested { get; }
}