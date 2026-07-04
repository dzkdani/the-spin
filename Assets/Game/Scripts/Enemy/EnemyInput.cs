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
    bool active = true;
    float dashChance = 0.3f;

    private void Start()
    {
        if(target == null)
        {
            target =
                GameObject.FindWithTag("Player").transform;
        }
    }

    private void Update()
    {
        if (target == null || !active)
            return;

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

    public void DisableAI()
    {
        active = false;
    }
}