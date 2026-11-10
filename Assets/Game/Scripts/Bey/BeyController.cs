using UnityEngine;

[RequireComponent(typeof(BeyData))]
[RequireComponent(typeof(BeyPhysics))]
[RequireComponent(typeof(BeyCombat))]
public class BeyController : MonoBehaviour
{
    public BeyData Data { get; private set; }
    public BeyPhysics Physics { get; private set; }
    public BeyCombat Combat { get; private set; }
    public BeyVisual Visual { get; private set; }
    public IBeyInput Input { get; private set; }

    private AbilityRunner abilityRunner;

    private void Awake()
    {
        Data = GetComponent<BeyData>();
        Physics = GetComponent<BeyPhysics>();
        Combat = GetComponent<BeyCombat>();
        Input = GetComponent<IBeyInput>();
        Visual = GetComponentInChildren<BeyVisual>();
        abilityRunner = GetComponent<AbilityRunner>();

        if (Data == null || Physics == null || Combat == null || Input == null || Visual == null)
        {
            Debug.LogError(
                "BeyController requires BeyData, BeyPhysics, BeyCombat, an IBeyInput, and a child BeyVisual.",
                this);

            enabled = false;

            if (Physics != null)
                Physics.enabled = false;

            if (Combat != null)
                Combat.enabled = false;

            return;
        }

        Physics.Initialize(Data, Input);
        Combat.Initialize(Data, Physics);
        Visual.Initialize(Data);

        if (abilityRunner == null)
            abilityRunner = gameObject.AddComponent<AbilityRunner>();

        abilityRunner.Initialize(Data, Physics, Combat, Input);
    }

    private void Update()
    {
        IBattleStateGate gate = ServiceLocator.Get<IBattleStateGate>();

        if (gate != null && !gate.CanReceiveGameplayInput)
            return;

        if (!Input.DashRequested)
            return;

        if (Physics.Dash(Input.MoveTarget - (Vector2)transform.position))
            Combat.AddChargedHit();
    }
}
