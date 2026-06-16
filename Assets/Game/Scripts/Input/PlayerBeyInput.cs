using UnityEngine;

public class PlayerBeyInput : MonoBehaviour, IBeyInput
{
    public Vector2 MoveTarget { get; private set; }

    public bool ClickDown { get; private set; }

    Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        MoveTarget =
            cam.ScreenToWorldPoint(Input.mousePosition);

        ClickDown =
            Input.GetMouseButtonDown(0);
    }
}