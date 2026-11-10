using UnityEngine;

public class EnemyInput : MonoBehaviour, IBeyInput
{
    [SerializeField] private BeyController target;
    [SerializeField] private AiProfile profile;
    [SerializeField] private int seed = 0;

    public Vector2 MoveTarget { get; private set; }

    public bool DashRequested { get; private set; }

    public int ConsumeAbilityRequest()
    {
        return -1;
    }

    public bool IsWindingUp => windUpTimer > 0f;

    public string CurrentBehavior =>
        brain != null ? brain.CurrentBehavior : "none";

    private BeyData data;
    private IRandom random;
    private EnemyBrain brain;
    private float windUpTimer;
    private float dashCooldown;

    private void Awake()
    {
        data = GetComponent<BeyData>();

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

    private void Update()
    {
        IBattleStateGate gate = ServiceLocator.Get<IBattleStateGate>();

        if (gate != null && !gate.CanReceiveGameplayInput)
        {
            DashRequested = false;
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
        Vector2 selfPosition = transform.position;
        Vector2 opponentPosition = target.transform.position;

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
            Time.time);
    }
}
