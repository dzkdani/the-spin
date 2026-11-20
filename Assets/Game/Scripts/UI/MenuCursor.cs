using UnityEngine;

public sealed class MenuCursor : MonoBehaviour
{
    private const int CursorSize = 64;

    private Texture2D cursor;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Ensure()
    {
        if (FindFirstObjectByType<MenuCursor>() != null)
            return;

        GameObject host = new GameObject("MenuCursor");
        DontDestroyOnLoad(host);

        MenuCursor component = host.AddComponent<MenuCursor>();
        component.cursor = CreateCursor();
        component.Apply();
    }

    private static Texture2D CreateCursor()
    {
        Texture2D texture = new Texture2D(
            CursorSize,
            CursorSize,
            TextureFormat.RGBA32,
            false);

        texture.name = "MenuCursor";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[CursorSize * CursorSize];
        Color dark = new Color(0.06f, 0.06f, 0.08f, 1f);
        Color light = Color.white;
        Color clear = new Color(0f, 0f, 0f, 0f);
        Vector2 center = new Vector2(
            (CursorSize - 1) * 0.5f,
            (CursorSize - 1) * 0.5f);

        for (int y = 0; y < CursorSize; y++)
        {
            for (int x = 0; x < CursorSize; x++)
            {
                float distance = Vector2.Distance(
                    new Vector2(x, y),
                    center);

                Color color = clear;

                if (distance <= 2f)
                    color = light;
                else if (distance <= 5f)
                    color = dark;
                else if (distance >= 18f && distance < 22f)
                    color = light;
                else if (distance >= 22f && distance <= 26f)
                    color = dark;

                pixels[y * CursorSize + x] = color;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return texture;
    }

    private void OnDestroy()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void Apply()
    {
        Vector2 hotspot = new Vector2(
            cursor.width * 0.5f,
            cursor.height * 0.5f);

        Cursor.SetCursor(cursor, hotspot, CursorMode.Auto);
    }
}
