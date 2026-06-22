using UnityEngine;

public class BeyController : MonoBehaviour
{
    public BeyData Data { get; private set; }
    public BeyPhysics Physics { get; private set; }
    public BeyCombat Combat { get; private set; }
    public BeyVisual Visual { get; private set; }
    public IBeyInput Input { get; private set; }
    public BeyUI UI { get; private set; }

    private void Awake()
    {
        Data = GetComponent<BeyData>();
        Physics = GetComponent<BeyPhysics>();
        Combat = GetComponent<BeyCombat>();
        Input = GetComponent<IBeyInput>();
        Visual = GetComponentInChildren<BeyVisual>();
        UI = GetComponentInChildren<BeyUI>();

        Physics.Initialize(Data, Input);
        Combat.Initialize(Data, Physics);
        Visual.Initialize(Data);
        UI.Initialize(Data);
    }

    private void Update()
    {
        if (Input.ClickDown)
        {
            OnClickAction();
        }
    }

    private void OnClickAction()
    {
        Physics.Dash(Input.MoveTarget - (Vector2)transform.position);
        Combat.AddChargedHit();
    }
}
