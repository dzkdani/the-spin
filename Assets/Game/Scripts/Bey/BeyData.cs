using UnityEngine;
using System;

public class BeyData : MonoBehaviour
{
    [field: SerializeField]
    public BeyStats Base { get; private set; }

    [SerializeField] private BeyBlueprint blueprint;

    public BeyBlueprint Blueprint => blueprint;

    public StatBlock Stats { get; } = new StatBlock();

    public ElementStack Elements { get; } = new ElementStack();
    public event Action<float> OnRPMChanged;
    public event Action OnRPMDepleted;

    private float MaxRPM => Base != null ? Base.maxRPM : 0f;
    public float RPMNormalized => MaxRPM > 0f ? CurrentRPM / MaxRPM : 0f;
    public float CurrentRPM { get; private set; }


    private void Awake()
    {
        BeyStats resolved = blueprint != null && blueprint.chassis != null
            ? blueprint.chassis
            : Base;

        if (resolved == null)
        {
            Debug.LogError("BeyData requires a BeyStats reference.", this);
            return;
        }

        if (resolved.maxRPM <= 0f)
        {
            Debug.LogError("BeyData requires BeyStats.maxRPM to be greater than zero.", this);
            return;
        }

        Base = resolved;
        CurrentRPM = Base.maxRPM;

        Stats.SetBase(StatId.Attack, Base.attack);
        Stats.SetBase(StatId.Weight, Base.weight);
        Stats.SetBase(StatId.Defense, 0f);
        Stats.SetBase(StatId.RpmDecay, 1f);
        Stats.SetBase(StatId.MatchupScale, 1f);
        Stats.SetBase(StatId.TipFriction, 1f);
        Stats.SetBase(StatId.AbilityCost, 1f);
        Stats.SetBase(StatId.AbilityCooldown, 1f);
        Stats.SetBase(StatId.AbilityPower, 1f);

        BeyTypeDefinition type = blueprint != null && blueprint.type != null
            ? blueprint.type
            : Base.type;

        ElementDefinition element = blueprint != null && blueprint.baseElement != null
            ? blueprint.baseElement
            : Base.baseElement;

        if (type != null)
            type.ApplyTo(Stats, 0f);

        if (element != null)
        {
            Elements.Add(new ElementClaim(
                element,
                0,
                "base:chassis",
                float.PositiveInfinity,
                0));
        }

        OnRPMChanged?.Invoke(CurrentRPM);
    }

    private void Update()
    {
        ApplyDecay(Time.deltaTime);
    }

    public void ApplyDecay(float deltaTime)
    {
        if (Base == null ||
            Base.maxRPM <= 0f ||
            Base.rpmDecayRate <= 0f ||
            CurrentRPM <= 0f)
        {
            return;
        }

        float amount = Base.maxRPM *
            Base.rpmDecayRate *
            Stats.Get(StatId.RpmDecay) *
            deltaTime;

        if (amount > 0f)
            ReduceRPM(amount);
    }

    public void ReduceRPM(float amount)
    {
        SetRPM(CurrentRPM - amount);
    }

    public void AddRPM(float amount)
    {
        SetRPM(CurrentRPM + amount);
    }

    private void SetRPM(float value)
    {
        if (Base == null || MaxRPM <= 0f)
            return;

        float newRPM = Mathf.Clamp(value, 0, MaxRPM);

        if (Mathf.Approximately(newRPM, CurrentRPM))
            return;

        bool wasAlive = CurrentRPM > 0;

        CurrentRPM = newRPM;

        OnRPMChanged?.Invoke(CurrentRPM);

        BattleEvents.RaiseRpmChanged(this, CurrentRPM);

        if (wasAlive && CurrentRPM <= 0)
        {
            OnRPMDepleted?.Invoke();
        }
    }
}
