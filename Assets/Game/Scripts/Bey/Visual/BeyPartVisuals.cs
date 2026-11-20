using System.Collections.Generic;
using UnityEngine;

public sealed class BeyPartVisuals : MonoBehaviour
{
    private static Sprite ringSprite;
    private static Sprite dotSprite;

    private readonly List<GameObject> markers = new List<GameObject>();
    private BeyData data;
    private Transform pivot;
    private Transform root;

    private void Awake()
    {
        data = GetComponent<BeyData>();

        BeyVisual visual = GetComponentInChildren<BeyVisual>();
        pivot = visual != null && visual.Pivot != null
            ? visual.Pivot
            : transform;

        root = new GameObject("PartMarkers").transform;
        root.SetParent(pivot, false);
    }

    private void OnEnable()
    {
        if (data != null)
            data.OnBlueprintChanged += Rebuild;

        Rebuild();
    }

    private void OnDisable()
    {
        if (data != null)
            data.OnBlueprintChanged -= Rebuild;
    }

    private void Rebuild()
    {
        for (int i = markers.Count - 1; i >= 0; i--)
            Destroy(markers[i]);

        markers.Clear();

        if (data == null || data.Blueprint == null || root == null)
            return;

        float lossy = Mathf.Abs(pivot.lossyScale.x);

        if (lossy < 0.0001f)
            lossy = 1f;

        List<PartDefinition> parts = data.Blueprint.parts;

        for (int i = 0; i < parts.Count; i++)
        {
            PartDefinition part = parts[i];

            if (part == null || part.slot == null || string.IsNullOrEmpty(part.slot.id))
                continue;

            Color color = part.elementClaim != null
                ? part.elementClaim.color
                : new Color(0.75f, 0.78f, 0.85f, 1f);

            if (part.slot.id == "blade")
                BuildBlade(part, color, lossy);
            else if (part.slot.id == "ratchet")
                AddMarker(GetRing(), color, 1.35f, Vector3.zero, lossy);
            else if (part.slot.id == "bit")
                AddMarker(
                    GetDot(),
                    Color.Lerp(color, Color.white, 0.25f),
                    0.34f,
                    Vector3.zero,
                    lossy);
        }
    }

    private void BuildBlade(PartDefinition part, Color color, float lossy)
    {
        AddMarker(GetRing(), color, 1.9f, Vector3.zero, lossy);

        int notches = 3 + Mathf.Abs(part.id.GetHashCode()) % 3;
        Color notchColor = Color.Lerp(color, Color.white, 0.5f);

        for (int i = 0; i < notches; i++)
        {
            float angle = 2f * Mathf.PI * i / notches;
            Vector3 position = new Vector3(
                Mathf.Cos(angle) * 0.78f,
                Mathf.Sin(angle) * 0.78f,
                0f);

            AddMarker(GetDot(), notchColor, 0.13f, position, lossy);
        }
    }

    private void AddMarker(
        Sprite sprite,
        Color color,
        float worldSize,
        Vector3 worldOffset,
        float lossy)
    {
        GameObject marker = new GameObject("Marker");
        marker.transform.SetParent(root, false);
        marker.transform.localPosition = new Vector3(
            worldOffset.x / lossy,
            worldOffset.y / lossy,
            0f);
        marker.transform.localScale = Vector3.one * (worldSize / lossy);

        SpriteRenderer renderer = marker.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = 6;

        markers.Add(marker);
    }

    private static Sprite GetRing()
    {
        if (ringSprite != null)
            return ringSprite;

        ringSprite = CreateSprite(128, 46f, 58f);
        return ringSprite;
    }

    private static Sprite GetDot()
    {
        if (dotSprite != null)
            return dotSprite;

        dotSprite = CreateSprite(64, 0f, 30f);
        return dotSprite;
    }

    private static Sprite CreateSprite(int size, float inner, float outer)
    {
        Texture2D texture = new Texture2D(
            size, size, TextureFormat.RGBA32, false);

        Color[] pixels = new Color[size * size];
        Vector2 center = new Vector2(
            (size - 1) * 0.5f,
            (size - 1) * 0.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(
                    new Vector2(x, y), center);

                float alpha = distance >= inner && distance <= outer
                    ? 1f
                    : 0f;

                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            100f);
    }
}
