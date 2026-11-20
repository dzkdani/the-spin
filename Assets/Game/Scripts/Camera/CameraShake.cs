using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private float decay = 1.8f;
    [SerializeField] private float maxOffset = 0.35f;

    private Vector3 originalLocalPos;
    private float trauma;
    private float seed;

    private void Awake()
    {
        ServiceLocator.Register(this);

        originalLocalPos = transform.localPosition;
        seed = Random.value * 10f;
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister(this);
    }

    private void OnEnable()
    {
        BattleEvents.ImpactResolved += OnImpactResolved;
        BattleEvents.LaunchResolved += OnLaunchResolved;
    }

    private void OnDisable()
    {
        BattleEvents.ImpactResolved -= OnImpactResolved;
        BattleEvents.LaunchResolved -= OnLaunchResolved;
    }

    private void OnImpactResolved(ImpactEvent impact)
    {
        Shake(impact.Shake);
    }

    private void OnLaunchResolved(BeyData bey, float power, bool playerLaunch)
    {
        Shake(playerLaunch ? 0.12f + 0.4f * power : 0.05f * power);
    }

    public void Shake(float amount)
    {
        if (maxOffset <= 0f)
            return;

        trauma = Mathf.Clamp01(trauma + amount / maxOffset);
    }

    private void LateUpdate()
    {
        if (trauma <= 0f)
        {
            transform.localPosition = originalLocalPos;
            return;
        }

        trauma = Mathf.Max(0f, trauma - decay * Time.unscaledDeltaTime);

        float shake = trauma * trauma;
        float t = Time.unscaledTime * 30f + seed;

        transform.localPosition = originalLocalPos + new Vector3(
            Mathf.Sin(t * 1.7f) * maxOffset * shake,
            Mathf.Sin(t * 2.3f) * maxOffset * shake,
            0f);
    }
}
