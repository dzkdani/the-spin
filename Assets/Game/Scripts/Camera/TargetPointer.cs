using UnityEngine;

public sealed class TargetPointer : MonoBehaviour
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float hueSpeed = 0.35f;
    [SerializeField] private float spinSpeed = 220f;
    [SerializeField] private float height = 0.5f;

    private SpriteRenderer marker;
    private bool launchActive;
    private float spin;

    private void Awake()
    {
        marker = GetComponent<SpriteRenderer>();

        if (marker == null)
            marker = gameObject.AddComponent<SpriteRenderer>();

        marker.sprite = sprite;
        marker.sortingOrder = 40;
    }

    private void OnEnable()
    {
        BattleEvents.LaunchStarted += OnLaunchStarted;
        BattleEvents.LaunchResolved += OnLaunchResolved;
    }

    private void OnDisable()
    {
        BattleEvents.LaunchStarted -= OnLaunchStarted;
        BattleEvents.LaunchResolved -= OnLaunchResolved;
        Cursor.visible = true;
    }

    private void OnLaunchStarted(BeyData bey)
    {
        launchActive = true;
    }

    private void OnLaunchResolved(BeyData bey, float power, bool playerLaunch)
    {
        launchActive = false;
    }

    private void Update()
    {
        if (targetCamera == null)
            return;

        bool canInput =
            ServiceLocator.Get<IBattleStateGate>()?.CanReceiveGameplayInput ?? false;

        bool visible =
            !launchActive && canInput && Time.timeScale > 0f;

        marker.enabled = visible;
        Cursor.visible = !visible;

        if (!visible)
            return;

        Vector3 world = targetCamera.ScreenToWorldPoint(Input.mousePosition);
        world.z = 0f;

        transform.position = world + Vector3.up * height;

        spin = (spin + spinSpeed * Time.deltaTime) % 360f;
        transform.rotation = Quaternion.Euler(0f, spin, 0f);

        marker.color = Color.HSVToRGB(
            Mathf.Repeat(Time.unscaledTime * hueSpeed, 1f),
            0.85f,
            1f);
    }
}
