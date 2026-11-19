using UnityEngine;

public class EnemyInput : MonoBehaviour, IBeyInput, IBattleStateGateConsumer
{
    [SerializeField] private BeyController target;
    [SerializeField] private AiProfile profile;
    [SerializeField] private int seed = 0;

    public Vector2 MoveTarget { get; private set; }

    public bool DashRequested { get; private set; }

    public void SetGate(IBattleStateGate newGate)
    {
        gate = newGate;
    }

    public int ConsumeAbilityRequest()
    {
        int slot = abilityRequest;
        abilityRequest = -1;
        return slot;
    }

    public bool IsWindingUp => windUpTimer > 0f;

    public string CurrentBehavior =>
        brain != null ? brain.CurrentBehavior : "none";

    private BeyData data;
    private IRandom random;
    private EnemyBrain brain;
    private AbilityRunner runner;
    private IBattleStateGate gate;
    private Vector2 arenaCenter;
    private Vector2 arenaRadii;
    private float windUpTimer;
    private float dashCooldown;
    private float abilityCooldown;
    private int abilityRequest = -1;

    public void SetProfile(AiProfile newProfile)
    {
        profile = newProfile;
        brain = new EnemyBrain(profile, random);
    }

    private void Awake()
    {
        data = GetComponent<BeyData>();
        runner = GetComponent<AbilityRunner>();

        random = seed == 0
            ? (IRandom)new UnityRandom()
            : new SeededRandom(seed);

        if (profile == null)
            profile = ScriptableObject.CreateInstance<AiProfile>();

        brain = new EnemyBrain(profile, random);
    }

    public void SetTarget(BeyController newTarget)
    {
        target = newTarget;
    }

    public void SetArena(Vector2 center, Vector2 radii)
    {
        arenaCenter = center;
        arenaRadii = radii;
    }

    private void Update()
    {
        if (gate != null && !gate.CanReceiveGameplayInput)
        {
            DashRequested = false;
            abilityRequest = -1;
            return;
        }

        if (target == null || target.Data == null || data == null)
        {
            DashRequested = false;
            return;
        }

        BattleSnapshot snapshot = BuildSnapshot();
        AiCommand command = brain.Tick(snapshot, Time.deltaTime);

        MoveTarget = command.MoveTarget;
        DashRequested = false;

        abilityCooldown -= Time.deltaTime;

        if (command.AbilitySlot >= 0 && abilityCooldown <= 0f)
        {
            abilityRequest = command.AbilitySlot;
            abilityCooldown = 1.5f;
        }

        dashCooldown -= Time.deltaTime;

        if (windUpTimer > 0f)
        {
            windUpTimer -= Time.deltaTime;

            if (windUpTimer <= 0f)
            {
                DashRequested = true;

                dashCooldown = random.Range(
                    profile.dashIntervalMin,
                    profile.dashIntervalMax);
            }

            return;
        }

        if (command.RequestDash && dashCooldown <= 0f)
        {
            windUpTimer = profile.dashWindUp;
            BattleEvents.RaiseDashWindUpStarted(data);
        }
    }

    private BattleSnapshot BuildSnapshot()
    {
        if (runner == null)
            runner = GetComponent<AbilityRunner>();

        Vector2 selfPosition = transform.position;
        Vector2 opponentPosition = target.transform.position;

        int activeCount = runner != null ? runner.ActiveCount : 0;
        bool[] activeReady = new bool[activeCount];
        float[] activeCooldowns = new float[activeCount];

        for (int i = 0; i < activeCount; i++)
        {
            activeReady[i] = runner.IsActiveReady(i);
            activeCooldowns[i] = runner.GetActiveCooldown(i);
        }

        return new BattleSnapshot(
            data.RPMNormalized,
            target.Data.RPMNormalized,
            selfPosition,
            opponentPosition,
            Vector2.Distance(selfPosition, opponentPosition),
            data.Elements.Current,
            target.Data.Elements.Current,
            target.Physics != null && target.Physics.IsDashing,
            MatchupResolver.Get(
                ContentRegistry.Matchups,
                data.Elements.Current,
                target.Data.Elements.Current),
            Time.time,
            activeReady,
            activeCooldowns,
            arenaCenter,
            arenaRadii);
    }
}
