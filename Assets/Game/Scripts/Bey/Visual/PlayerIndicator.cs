using UnityEngine;

public sealed class PlayerIndicator : MonoBehaviour
{
    [SerializeField] private Sprite glowSprite;
    [SerializeField] private float ringWorldSize = 2.2f;
    [SerializeField] private int pulseCount = 3;
    [SerializeField] private float pulseSeconds = 0.35f;
    [SerializeField] private float peakAlpha = 0.85f;
    [SerializeField] private float peakScale = 1.35f;

    private BeyData data;
    private SpriteRenderer ring;
    private float elapsed;
    private int pulsesDone;
    private bool running;

    private void Awake()
    {
        data = GetComponent<BeyData>();

        GameObject child = new GameObject("PlayerGlow");
        child.transform.SetParent(transform, false);
        child.transform.localPosition = Vector3.zero;

        ring = child.AddComponent<SpriteRenderer>();
        ring.sprite = glowSprite;
        ring.sortingOrder = 4;
        ring.enabled = false;
    }

    private void OnEnable()
    {
        if (ring == null)
            return;

        Color color = data != null && data.Elements.Current != null
            ? data.Elements.Current.color
            : Color.white;

        color.a = peakAlpha;
        ring.color = color;
        ring.enabled = true;

        elapsed = 0f;
        pulsesDone = 0;
        running = true;
        ApplyScale(1f);
    }

    private void OnDisable()
    {
        running = false;

        if (ring != null)
            ring.enabled = false;
    }

    private void Update()
    {
        if (!running || ring == null)
            return;

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / pulseSeconds);

        ApplyScale(Mathf.Lerp(1f, peakScale, t));

        Color color = ring.color;
        color.a = Mathf.Lerp(peakAlpha, 0f, t);
        ring.color = color;

        if (elapsed < pulseSeconds)
            return;

        elapsed -= pulseSeconds;
        pulsesDone++;

        if (pulsesDone >= pulseCount)
        {
            running = false;
            ring.enabled = false;
        }
    }

    private void ApplyScale(float pulse)
    {
        Vector3 lossy = transform.lossyScale;

        float x = ringWorldSize;
        float y = ringWorldSize;

        if (Mathf.Abs(lossy.x) > 0.0001f)
            x /= lossy.x;

        if (Mathf.Abs(lossy.y) > 0.0001f)
            y /= lossy.y;

        ring.transform.localScale = new Vector3(x * pulse, y * pulse, 1f);
    }
}
