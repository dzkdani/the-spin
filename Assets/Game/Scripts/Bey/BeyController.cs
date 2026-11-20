using System.Collections.Generic;
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

    public AbilityRunner Abilities => abilityRunner;

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

        if (GetComponent<BeyPartVisuals>() == null)
            gameObject.AddComponent<BeyPartVisuals>();
    }

    private void Start()
    {
        if (Data == null || abilityRunner == null)
            return;

        BeyBuilder.Build(Data, abilityRunner);

        IBattleStateGate gate = ServiceLocator.Get<IBattleStateGate>();

        if (Input is IBattleStateGateConsumer consumer)
            consumer.SetGate(gate);
    }

    public bool ApplyBlueprint(BeyBlueprint blueprint)
    {
        if (blueprint == null || Data == null || abilityRunner == null)
            return false;

        List<string> errors = BeyBlueprintValidator.Validate(
            blueprint,
            ContentRegistry.Tuning);

        if (errors.Count > 0)
        {
            for (int i = 0; i < errors.Count; i++)
                Debug.LogError("Blueprint: " + errors[i], this);

            return false;
        }

        abilityRunner.ClearAll();
        Data.Configure(blueprint);
        BeyBuilder.Build(Data, abilityRunner);

        return true;
    }

    private void Update()
    {
        IBattleStateGate gate = ServiceLocator.Get<IBattleStateGate>();

        if (gate != null && !gate.CanReceiveGameplayInput)
            return;

        int abilitySlot = Input.ConsumeAbilityRequest();

        if (abilitySlot >= 0 && abilityRunner != null)
            abilityRunner.TryActivate(abilitySlot);

        if (!Input.DashRequested)
            return;

        if (Physics.Dash(Input.MoveTarget - (Vector2)transform.position))
            Combat.AddChargedHit();
    }
}
