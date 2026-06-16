using UnityEngine;

public interface IBeyInput
{
    Vector2 MoveTarget { get; }
    bool ClickDown { get; }
}