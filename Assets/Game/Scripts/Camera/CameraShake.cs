using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private float damping = 25f;

    private Vector3 originalLocalPos;
    private float intensity;

    private void Awake()
    {
        ServiceLocator.Register(this);

        originalLocalPos = transform.localPosition;
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister(this);
    }

    private void LateUpdate()
    {
        if (intensity > 0f)
        {
            transform.localPosition =
                originalLocalPos +
                (Vector3)Random.insideUnitCircle * intensity;

            intensity = Mathf.MoveTowards(
                intensity,
                0f,
                damping * Time.deltaTime);
        }
        else
        {
            transform.localPosition = originalLocalPos;
        }
    }

    private void OnEnable()
    {
        BattleEvents.ImpactResolved += OnImpactResolved;
    }

    private void OnDisable()
    {
        BattleEvents.ImpactResolved -= OnImpactResolved;
    }

    private void OnImpactResolved(ImpactEvent impact)
    {
        Shake(impact.Shake);
    }

    public void Shake(float amount)
    {
        intensity = Mathf.Max(intensity, amount);
    }
}