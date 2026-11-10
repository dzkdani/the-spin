using UnityEngine;

public class EnemyInput : MonoBehaviour, IBeyInput
{
    [SerializeField] private Transform target;
    [SerializeField] private AiProfile profile;
    [SerializeField] private int seed = 0;

    public Vector2 MoveTarget { get; private set; }
    public bool DashRequested => shouldDash;

    bool shouldDash;
    float dashTimer;
    Vector2 offset;
    float offsetTimer;
    float dashChance;
    private IRandom random;

    private void Awake()
    {
        random = seed == 0
            ? (IRandom)new UnityRandom()
            : new SeededRandom(seed);

        if (profile == null)
            profile = ScriptableObject.CreateInstance<AiProfile>();
    }

    private void Start()
    {
        if(target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");

            if (player != null)
                target = player.transform;
            else
                Debug.LogError("EnemyInput could not find a GameObject with the Player tag.");
        }
    }

    private void Update()
    {
        IBattleStateGate gate = ServiceLocator.Get<IBattleStateGate>();

        if (gate != null && !gate.CanReceiveGameplayInput)
        {
            shouldDash = false;
            return;
        }

        if (target == null)
        {
            shouldDash = false;
            return;
        }

        // Update wander offset every second
        offsetTimer -= Time.deltaTime;

        if (offsetTimer <= 0f)
        {
            offset = random.InsideUnitCircle() * profile.jitterRadius;
            offsetTimer = profile.jitterInterval;
        }

        MoveTarget = (Vector2)target.position + offset;

        // Increase dash chance when far away
        float distance = Vector2.Distance(transform.position, target.position);

        dashChance = distance > profile.farDistance
            ? profile.dashChanceFar
            : profile.dashChanceNear;

        // Dash timer
        dashTimer -= Time.deltaTime;

        shouldDash = false;

        if (dashTimer <= 0f)
        {
            shouldDash = random.Value() < dashChance;
            dashTimer = random.Range(
                profile.dashIntervalMin,
                profile.dashIntervalMax);
        }
    }
}
