using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class LaunchOverlayPrefabBuilder
{
    private const string PrefabPath =
        "Assets/Game/Prefabs/LaunchOverlay.prefab";

    [MenuItem("Tools/Build Launch Overlay Prefab")]
    public static void Build()
    {
        Sprite background = AssetDatabase.GetBuiltinExtraResource<Sprite>(
            "UI/Skin/Background.psd");

        Sprite uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(
            "UI/Skin/UISprite.psd");

        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/Game/Art/Font/Koulen-Regular SDF.asset");

        GameObject root = new GameObject(
            "LaunchOverlay",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Image darken = CreateImage(
            "Darken", root.transform, background,
            new Color(0f, 0f, 0f, 0.55f));

        Stretch(darken.rectTransform);

        Image panel = CreateImage(
            "Panel", root.transform, uiSprite,
            new Color(0.12f, 0.1f, 0.08f, 0.96f));

        RectTransform panelRect = panel.rectTransform;
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(420f, 260f);
        panelRect.anchoredPosition = Vector2.zero;

        TMP_Text title = CreateText(
            "Title", panel.transform, font, "LAUNCH", 34f);

        Place(title.rectTransform, new Vector2(380f, 44f), new Vector2(0f, 78f));

        Image powerBg = CreateImage(
            "PowerBg", panel.transform, uiSprite,
            new Color(0.25f, 0.2f, 0.15f, 1f));

        Place(powerBg.rectTransform, new Vector2(340f, 28f), new Vector2(0f, -6f));

        Image powerFill = CreateImage(
            "PowerFill", panel.transform, uiSprite,
            new Color(0.95f, 0.75f, 0.2f, 1f));

        Place(powerFill.rectTransform, new Vector2(340f, 28f), new Vector2(0f, -6f));
        powerFill.type = Image.Type.Filled;
        powerFill.fillMethod = Image.FillMethod.Horizontal;
        powerFill.fillOrigin = 0;
        powerFill.fillAmount = 0f;

        TMP_Text hint = CreateText(
            "Hint", panel.transform, font,
            "HOLD · PULL DOWN · RELEASE", 24f);

        Place(hint.rectTransform, new Vector2(380f, 44f), new Vector2(0f, -70f));

        LaunchOverlay overlay = root.AddComponent<LaunchOverlay>();

        SerializedObject serialized = new SerializedObject(overlay);
        serialized.FindProperty("powerFill").objectReferenceValue = powerFill;
        serialized.FindProperty("hint").objectReferenceValue = hint;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        Directory.CreateDirectory("Assets/Game/Prefabs");

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        AssetDatabase.Refresh();
        Debug.Log("Built " + PrefabPath);
    }

    [MenuItem("Tools/Wire Launch Overlay In BattleScene")]
    public static void WireScene()
    {
        UnityEngine.SceneManagement.Scene scene =
            EditorSceneManager.OpenScene(
                "Assets/Game/Scenes/BattleScene.unity",
                OpenSceneMode.Single);

        LaunchOverlay prefab = AssetDatabase.LoadAssetAtPath<LaunchOverlay>(
            PrefabPath);

        LaunchTuning tuning = AssetDatabase.LoadAssetAtPath<LaunchTuning>(
            "Assets/Game/Datas/LaunchTuning.asset");

        GameManager manager = Object.FindFirstObjectByType<GameManager>();
        Camera camera = Object.FindFirstObjectByType<Camera>();

        if (prefab == null || tuning == null || manager == null)
        {
            Debug.LogError("Launch wiring is missing its prefab, tuning, or GameManager.");
            return;
        }

        foreach (LaunchOverlay existing in Object.FindObjectsByType<LaunchOverlay>(
                     FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(existing.gameObject);
        }

        LaunchOverlay overlay = (LaunchOverlay)PrefabUtility.InstantiatePrefab(
            prefab, scene);

        overlay.name = "LaunchOverlay";
        overlay.transform.position = Vector3.zero;

        LaunchController controller = manager.GetComponent<LaunchController>();

        if (controller == null)
            controller = manager.gameObject.AddComponent<LaunchController>();

        SerializedObject controllerData = new SerializedObject(controller);
        controllerData.FindProperty("tuning").objectReferenceValue = tuning;
        controllerData.FindProperty("overlay").objectReferenceValue = overlay;
        controllerData.FindProperty("targetCamera").objectReferenceValue = camera;
        controllerData.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject managerData = new SerializedObject(manager);
        managerData.FindProperty("launch").objectReferenceValue = controller;
        managerData.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("Wired launch system into BattleScene");
    }

    private static Image CreateImage(
        string name,
        Transform parent,
        Sprite sprite,
        Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        Image image = go.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;

        return image;
    }

    private static TMP_Text CreateText(
        string name,
        Transform parent,
        TMP_FontAsset font,
        string text,
        float size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();

        if (font != null)
            label.font = font;

        label.text = text;
        label.fontSize = size;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;

        return label;
    }

    private static void Place(
        RectTransform rect,
        Vector2 size,
        Vector2 position)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
