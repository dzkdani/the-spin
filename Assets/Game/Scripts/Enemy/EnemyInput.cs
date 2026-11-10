using UnityEngine;

public class EnemyInput : MonoBehaviour, IBeyInput
{
    [SerializeField] private Transform target;
    [SerializeField] float wanderRadius = 1.2f;

    public Vector2 MoveTarget { get; private set; }
    public bool DashRequested => shouldDash;

    bool shouldDash;
    float dashTimer;
    Vector2 offset;
    float offsetTimer;
    float dashChance = 0.3f;

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
        GameUIManager ui = ServiceLocator.Get<GameUIManager>();

        if (ui != null && !ui.CanReceiveGameplayInput)
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
            offset = Random.insideUnitCircle * wanderRadius;
            offsetTimer = 1f;
        }

        MoveTarget = (Vector2)target.position + offset;

        // Increase dash chance when far away
        float distance = Vector2.Distance(transform.position, target.position);

        dashChance = distance > 4f ? 0.7f : 0.2f;

        // Dash timer
        dashTimer -= Time.deltaTime;

        shouldDash = false;

        if (dashTimer <= 0f)
        {
            shouldDash = Random.value < dashChance;
            dashTimer = Random.Range(1.5f, 3f);
        }
    }
}
