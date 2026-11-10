using System.Collections.Generic;
using UnityEngine;

public class AbilityRunner : MonoBehaviour, IBeyActions, IBeyView
{
    private sealed class Runtime
    {
        public AbilityDefinition Definition;
        public string SourceId;
        public bool AuraActive;
        public float AuraTimer;
        public ActiveState State;
        public float CastUntil;
        public float CooldownUntil;
    }

    private enum ActiveState
    {
        Ready,
        Casting,
        Cooling
    }

    private sealed class NullBeyView : IBeyView
    {
        public static readonly NullBeyView Instance = new NullBeyView();

        public ElementDefinition Element => null;

        public float RpmNormalized => 0f;

        public bool IsDashing => false;
    }

    private readonly List<Runtime> abilities = new List<Runtime>();

    private BeyData data;
    private BeyPhysics physics;
    private BeyCombat combat;
    private IBeyInput input;
    private IBeyView opponent = NullBeyView.Instance;
    private float now;

    public StatBlock Stats => data != null ? data.Stats : null;

    public ElementStack Elements => data != null ? data.Elements : null;

    public float RpmNormalized => data != null ? data.RPMNormalized : 0f;

    public float MaxRpm =>
        data != null && data.Base != null
            ? data.Base.maxRPM
            : 0f;

    public ElementDefinition Element =>
        data != null
            ? data.Elements.Current
            : null;

    public bool IsDashing => physics != null && physics.IsDashing;

    public string OwnerId => name;

    public void Initialize(
        BeyData data,
        BeyPhysics physics,
        BeyCombat combat,
        IBeyInput input)
    {
        this.data = data;
        this.physics = physics;
        this.combat = combat;
        this.input = input;

        Subscribe();
    }

    private void Start()
    {
        InstallInnate();
    }

    public void SetOpponent(IBeyView view)
    {
        opponent = view ?? NullBeyView.Instance;
    }

    private void OnDestroy()
    {
        Unsubscribe();

        for (int i = 0; i < abilities.Count; i++)
            RemoveEffects(abilities[i].SourceId);
    }

    private void Update()
    {
        if (data == null)
            return;

        now = Time.time;

        TickActives();
        TickAuras(Time.deltaTime);

        data.Stats.Tick(now);
        data.Elements.Tick(now);

        if (combat != null)
            combat.TickPending(now);
    }

    public bool TryActivate(int slot)
    {
        List<Runtime> actives = new List<Runtime>();

        for (int i = 0; i < abilities.Count; i++)
        {
            if (abilities[i].Definition.kind == AbilityKind.Active)
                actives.Add(abilities[i]);
        }

        if (slot < 0 || slot >= actives.Count)
            return false;

        return TryActivate(actives[slot]);
    }

    public bool Install(AbilityDefinition definition, string sourceId)
    {
        if (definition == null || string.IsNullOrEmpty(sourceId))
            return false;

        if (HasAbility(definition.id))
            return false;

        if (!WithinLimits(definition, sourceId))
        {
            Debug.LogError(
                $"Ability '{definition.id}' exceeds the limits for source '{sourceId}'.",
                this);
            return false;
        }

        Runtime runtime = new Runtime
        {
            Definition = definition,
            SourceId = sourceId,
            State = ActiveState.Ready
        };

        abilities.Add(runtime);

        if (definition.kind == AbilityKind.Passive &&
            definition.mode == PassiveMode.Triggered &&
            (definition.trigger == TriggerId.Always ||
             definition.trigger == TriggerId.BattleStart))
        {
            EvaluateAndApply(runtime, null);
        }

        return true;
    }

    private void InstallInnate()
    {
        if (data == null || data.Base == null || data.Base.type == null)
            return;

        string abilityId = data.Base.type.innatePassiveId;

        if (string.IsNullOrEmpty(abilityId))
            return;

        AbilityDefinition definition =
            ContentRegistry.Get<AbilityDefinition>(abilityId);

        if (definition == null)
        {
            Debug.LogError($"Innate ability '{abilityId}' was not found in content.", this);
            return;
        }

        Install(definition, "innate:" + definition.id);
    }

    private bool HasAbility(string id)
    {
        for (int i = 0; i < abilities.Count; i++)
        {
            if (abilities[i].Definition.id == id)
                return true;
        }

        return false;
    }

    private bool WithinLimits(AbilityDefinition definition, string sourceId)
    {
        CombatTuning tuning = ContentRegistry.Tuning;

        int innatePassives = 0;
        int grantedPassives = 0;
        int grantedActives = 0;
        int equippedPassives = 0;
        int equippedActives = 0;

        for (int i = 0; i < abilities.Count; i++)
        {
            Runtime runtime = abilities[i];
            bool passive = runtime.Definition.kind == AbilityKind.Passive;

            if (runtime.SourceId.StartsWith("innate:"))
                innatePassives++;

            if (runtime.SourceId.StartsWith("grant:"))
            {
                if (passive) grantedPassives++;
                else grantedActives++;
            }

            if (runtime.SourceId.StartsWith("equipped:"))
            {
                if (passive) equippedPassives++;
                else equippedActives++;
            }
        }

        bool isPassive = definition.kind == AbilityKind.Passive;

        if (sourceId.StartsWith("innate:"))
            return isPassive && innatePassives < 1;

        if (sourceId.StartsWith("grant:"))
        {
            return isPassive
                ? grantedPassives < tuning.maxGrantedPassives
                : grantedActives < tuning.maxGrantedActives;
        }

        if (sourceId.StartsWith("equipped:"))
        {
            return isPassive
                ? equippedPassives < tuning.maxEquippedPassives
                : equippedActives < tuning.maxEquippedActives;
        }

        return true;
    }

    private void Subscribe()
    {
        BattleEvents.ImpactPending += OnImpactPending;
        BattleEvents.ImpactResolved += OnImpactResolved;
        BattleEvents.DashStarted += OnDashStarted;
        BattleEvents.DashEnded += OnDashEnded;
        BattleEvents.RpmChanged += OnRpmChanged;
    }

    private void Unsubscribe()
    {
        BattleEvents.ImpactPending -= OnImpactPending;
        BattleEvents.ImpactResolved -= OnImpactResolved;
        BattleEvents.DashStarted -= OnDashStarted;
        BattleEvents.DashEnded -= OnDashEnded;
        BattleEvents.RpmChanged -= OnRpmChanged;
    }

    private void OnImpactPending(ImpactEvent impact)
    {
        if (impact.Attacker == data)
            Trigger(TriggerId.ImpactPending, impact);
    }

    private void OnImpactResolved(ImpactEvent impact)
    {
        if (impact.Attacker == data)
            Trigger(TriggerId.ImpactDealt, impact);

        if (impact.Defender != data)
            return;

        Trigger(TriggerId.ImpactReceived, impact);
        InterruptCasting();
    }

    private void OnDashStarted(BeyData bey)
    {
        if (bey == data)
            Trigger(TriggerId.DashStarted, null);
    }

    private void OnDashEnded(BeyData bey)
    {
        if (bey == data)
            Trigger(TriggerId.DashEnded, null);
    }

    private void OnRpmChanged(BeyData bey, float rpm)
    {
        if (bey == data)
            Trigger(TriggerId.RpmChanged, null);
    }

    private void Trigger(TriggerId trigger, ImpactEvent? impact)
    {
        for (int i = 0; i < abilities.Count; i++)
        {
            Runtime runtime = abilities[i];
            AbilityDefinition definition = runtime.Definition;

            if (definition.kind != AbilityKind.Passive ||
                definition.mode != PassiveMode.Triggered ||
                definition.trigger != trigger)
            {
                continue;
            }

            EvaluateAndApply(runtime, impact);
        }
    }

    private void TickAuras(float deltaTime)
    {
        float interval = ContentRegistry.Tuning.auraTickInterval;

        for (int i = 0; i < abilities.Count; i++)
        {
            Runtime runtime = abilities[i];
            AbilityDefinition definition = runtime.Definition;

            if (definition.kind != AbilityKind.Passive ||
                definition.mode != PassiveMode.Aura)
            {
                continue;
            }

            runtime.AuraTimer -= deltaTime;

            if (runtime.AuraTimer > 0f)
                continue;

            runtime.AuraTimer = interval;

            bool holds = ConditionsHold(runtime, null);

            if (holds && !runtime.AuraActive)
            {
                ApplyEffects(runtime, null);
                runtime.AuraActive = true;
            }
            else if (!holds && runtime.AuraActive)
            {
                RemoveEffects(runtime.SourceId);
                runtime.AuraActive = false;
            }
        }
    }

    private void TickActives()
    {
        for (int i = 0; i < abilities.Count; i++)
        {
            Runtime runtime = abilities[i];

            if (runtime.Definition.kind != AbilityKind.Active)
                continue;

            if (runtime.State == ActiveState.Casting &&
                now >= runtime.CastUntil)
            {
                runtime.State = ActiveState.Cooling;
                ApplyEffects(runtime, null);
                BattleEvents.RaiseCastComplete(data, runtime.Definition.id);
            }

            if (runtime.State == ActiveState.Cooling &&
                now >= runtime.CooldownUntil)
            {
                runtime.State = ActiveState.Ready;
            }
        }
    }

    private bool TryActivate(Runtime runtime)
    {
        AbilityDefinition definition = runtime.Definition;

        if (definition.kind != AbilityKind.Active ||
            runtime.State != ActiveState.Ready)
        {
            return false;
        }

        if (IsDashing || AnyCasting())
            return false;

        string scope = ScopeFor(definition);
        float cost = definition.active.rpmCost *
            data.Stats.Get(StatId.AbilityCost, scope);

        if (data.RPMNormalized < cost)
            return false;

        data.ReduceRPM(definition.active.rpmCost * MaxRpm);

        float cooldown = definition.active.cooldown *
            data.Stats.Get(StatId.AbilityCooldown, scope);

        runtime.CooldownUntil = now + cooldown;
        runtime.CastUntil = now + definition.active.castTime;

        BattleEvents.RaiseAbilityActivated(data, definition.id);

        if (definition.active.castTime <= 0f)
        {
            runtime.State = ActiveState.Cooling;
            ApplyEffects(runtime, null);
            BattleEvents.RaiseCastComplete(data, definition.id);
        }
        else
        {
            runtime.State = ActiveState.Casting;
        }

        return true;
    }

    private bool AnyCasting()
    {
        for (int i = 0; i < abilities.Count; i++)
        {
            if (abilities[i].State == ActiveState.Casting)
                return true;
        }

        return false;
    }

    private void InterruptCasting()
    {
        for (int i = 0; i < abilities.Count; i++)
        {
            Runtime runtime = abilities[i];

            if (runtime.State != ActiveState.Casting)
                continue;

            if (runtime.Definition.active.interruptOnHit)
                runtime.State = ActiveState.Cooling;
        }
    }

    private void EvaluateAndApply(Runtime runtime, ImpactEvent? impact)
    {
        if (!ConditionsHold(runtime, impact))
            return;

        ApplyEffects(runtime, impact);
    }

    private bool ConditionsHold(Runtime runtime, ImpactEvent? impact)
    {
        AbilityContext context = CreateContext(runtime, impact);

        for (int i = 0; i < runtime.Definition.conditions.Count; i++)
        {
            if (!runtime.Definition.conditions[i].Evaluate(context))
                return false;
        }

        return true;
    }

    private void ApplyEffects(Runtime runtime, ImpactEvent? impact)
    {
        AbilityContext context = CreateContext(runtime, impact);

        for (int i = 0; i < runtime.Definition.effects.Count; i++)
            runtime.Definition.effects[i].Apply(context);
    }

    private AbilityContext CreateContext(Runtime runtime, ImpactEvent? impact)
    {
        string scope = ScopeFor(runtime.Definition);

        float power = data.Stats.Get(StatId.AbilityPower, scope);

        return new AbilityContext(
            this,
            opponent,
            impact,
            now,
            runtime.SourceId,
            power);
    }

    private static string ScopeFor(AbilityDefinition definition)
    {
        return definition.tags != null && definition.tags.Length > 0
            ? definition.tags[0]
            : null;
    }

    private void RemoveEffects(string sourceId)
    {
        if (data == null)
            return;

        data.Stats.RemoveBySource(sourceId);
        data.Elements.RemoveBySource(sourceId);

        if (combat != null)
            combat.RemovePendingBySource(sourceId);
    }

    public void AddPendingImpact(PendingImpactMod mod)
    {
        if (combat != null)
            combat.AddPendingImpact(mod);
    }

    public void DrainRpm(float amount)
    {
        if (data != null)
            data.ReduceRPM(amount);
    }

    public void RequestForcedDash(float power)
    {
        if (physics == null)
            return;

        Vector2 direction = input != null
            ? input.MoveTarget - (Vector2)transform.position
            : Vector2.right;

        physics.Dash(direction, power);
    }

    public void ClaimElement(
        ElementDefinition element,
        int priority,
        float expiresAt,
        string sourceId)
    {
        if (data != null && element != null)
            data.Elements.Add(element, priority, sourceId, expiresAt);
    }
}
