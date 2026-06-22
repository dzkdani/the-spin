using UnityEngine;

public class AIBeyInput : MonoBehaviour, IBeyInput
{
    Transform player;

    public Vector2 MoveTarget { get; private set; }

    public bool ClickDown { get; private set; }

    private void Start() {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        MoveTarget = Vector2.zero;
            // MoveTarget = player.position;
    }
}