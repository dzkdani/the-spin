using UnityEngine;

public sealed class MenuCursor : MonoBehaviour
{
    private const string ResourcePath = "MenuCursor";

    private Texture2D cursor;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Ensure()
    {
        if (FindFirstObjectByType<MenuCursor>() != null)
            return;

        Texture2D texture = Resources.Load<Texture2D>(ResourcePath);

        if (texture == null || !texture.isReadable)
        {
            Debug.LogError(
                "MenuCursor: MenuCursor.png must be RGBA32, readable, alphaIsTransparency, and mipmap-free. Run Tools/Fix Menu Cursor Import.",
                null);

            return;
        }

        GameObject host = new GameObject("MenuCursor");
        DontDestroyOnLoad(host);

        MenuCursor component = host.AddComponent<MenuCursor>();
        component.cursor = texture;
        component.Apply();
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
