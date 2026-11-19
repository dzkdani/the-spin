using UnityEngine;

public sealed class ArenaFraming : MonoBehaviour
{
    [SerializeField] private ArenaBounds arena;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float margin = 0.5f;

    private Camera cam;
    private Vector2 lastRadii;
    private float lastAspect = -1f;

    private void Awake()
    {
        cam = targetCamera != null ? targetCamera : GetComponent<Camera>();

        if (cam == null)
            cam = Camera.main;
    }

    private void Update()
    {
        if (cam == null || arena == null || !cam.orthographic)
            return;

        Vector2 radii = arena.Radii;
        float aspect = Mathf.Max(0.1f, Screen.width / (float)Screen.height);

        if (aspect == lastAspect && radii == lastRadii)
            return;

        lastAspect = aspect;
        lastRadii = radii;

        cam.orthographicSize = ComputeSize(radii, aspect, margin);
    }

    public static float ComputeSize(Vector2 radii, float aspect, float margin)
    {
        return Mathf.Max(
            radii.y + margin,
            (radii.x + margin) / Mathf.Max(0.1f, aspect));
    }
}
