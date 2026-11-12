using UnityEngine;

public class PlayerBeyInput : MonoBehaviour, IBeyInput, IBattleStateGateConsumer
{
    public Vector2 MoveTarget { get; private set; }

    public bool DashRequested { get; private set; }

    public void SetGate(IBattleStateGate newGate)
    {
        gate = newGate;
    }

    private int abilitySlotRequest = -1;

    public int ConsumeAbilityRequest()
    {
        int slot = abilitySlotRequest;
        abilitySlotRequest = -1;
        return slot;
    }

    private Camera cam;
    private IBattleStateGate gate;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (gate != null && !gate.CanReceiveGameplayInput)
        {
            DashRequested = false;
            return;
        }

        if (cam == null)
        {
            cam = Camera.main;

            if (cam == null)
            {
                DashRequested = false;
                return;
            }
        }

        MoveTarget =
            cam.ScreenToWorldPoint(Input.mousePosition);

        DashRequested =
            Input.GetMouseButtonDown(0);

        if (Input.GetKeyDown(KeyCode.Alpha1))
            abilitySlotRequest = 0;

        if (Input.GetKeyDown(KeyCode.Alpha2))
            abilitySlotRequest = 1;

        if (Input.GetKeyDown(KeyCode.Alpha3))
            abilitySlotRequest = 2;
    }
}
