using UnityEngine;

public sealed class ArenaBounds : MonoBehaviour
{
    [SerializeField] private Vector2 arenaCenter = Vector2.zero;
    [SerializeField] private Vector2 arenaRadii = new Vector2(8.2f, 4.4f);
    [SerializeField, Range(1f, 1.2f)] private float wallScale = 1.05f;
    [SerializeField, Range(8, 128)] private int wallSegments = 64;

    public Vector2 Center => arenaCenter;
    public Vector2 Radii => arenaRadii;

    private void Awake()
    {
        BuildWall();
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
            return;

        BuildWall();
    }

    public float NormalizedDistance(Vector2 point)
    {
        return NormalizedDistance(point, arenaCenter, arenaRadii);
    }

    public static float NormalizedDistance(
        Vector2 point,
        Vector2 center,
        Vector2 radii)
    {
        float rx = Mathf.Max(0.0001f, radii.x);
        float ry = Mathf.Max(0.0001f, radii.y);

        float dx = (point.x - center.x) / rx;
        float dy = (point.y - center.y) / ry;

        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    public static Vector2 Clamp(
        Vector2 point,
        Vector2 center,
        Vector2 radii,
        float margin)
    {
        float limit = Mathf.Max(0f, 1f - margin);
        float distance = NormalizedDistance(point, center, radii);

        if (distance <= limit)
            return point;

        float scale = limit / distance;

        return new Vector2(
            center.x + (point.x - center.x) * scale,
            center.y + (point.y - center.y) * scale);
    }

    public Vector2 Clamp(Vector2 point, float margin)
    {
        return Clamp(point, arenaCenter, arenaRadii, margin);
    }

    private void BuildWall()
    {
        EdgeCollider2D wall = GetComponent<EdgeCollider2D>();

        if (wall == null)
            wall = gameObject.AddComponent<EdgeCollider2D>();

        int count = Mathf.Max(8, wallSegments);
        Vector2[] points = new Vector2[count + 1];

        for (int i = 0; i <= count; i++)
        {
            float angle = i * Mathf.PI * 2f / count;
            Vector2 world = arenaCenter + new Vector2(
                Mathf.Cos(angle) * arenaRadii.x * wallScale,
                Mathf.Sin(angle) * arenaRadii.y * wallScale);

            points[i] = transform.InverseTransformPoint(world);
        }

        wall.points = points;
        wall.edgeRadius = 0.1f;
    }
}
