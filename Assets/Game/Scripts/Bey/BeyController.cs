using UnityEngine;

public class BeyController : MonoBehaviour
{
    public BeyData Data { get; private set; }
    public BeyPhysics Physics { get; private set; }
    public BeyCombat Combat { get; private set; }
    public BeyVisual Visual { get; private set; }
    public IBeyInput Input { get; private set; }

    private void Awake()
    {
        Data = GetComponent<BeyData>();
        Physics = GetComponent<BeyPhysics>();
        Combat = GetComponent<BeyCombat>();
        Input = GetComponent<IBeyInput>();
        Visual = GetComponentInChildren<BeyVisual>();

        Physics.Initialize(Data, Input);
        Combat.Initialize(Data, Physics);
        Visual.Initialize(Data);
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
        // TODO:
        // Dash
        // Boost
        // Spin Injection
        // Attack Trigger
    }
}
