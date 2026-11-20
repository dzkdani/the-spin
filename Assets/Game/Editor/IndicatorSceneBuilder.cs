using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class IndicatorSceneBuilder
{
    private const string TrianglePath =
        "Assets/Game/Art/UI/Triangle.png";

    private const string GlowRingPath =
        "Assets/Game/Art/UI/GlowRing.png";

    private const string PlayerPrefabPath =
        "Assets/Game/Prefabs/Player.prefab";

    private const string BattleScenePath =
        "Assets/Game/Scenes/BattleScene.unity";

    [MenuItem("Tools/Wire Indicators")]
    public static void Wire()
    {
        TextureImporter importer =
            AssetImporter.GetAtPath(TrianglePath) as TextureImporter;

        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }

        TextureImporter glowImporter =
            AssetImporter.GetAtPath(GlowRingPath) as TextureImporter;

        if (glowImporter != null)
        {
            glowImporter.textureType = TextureImporterType.Sprite;
            glowImporter.spriteImportMode = SpriteImportMode.Single;
            glowImporter.spritePixelsPerUnit = 100f;
            glowImporter.alphaIsTransparency = true;
            glowImporter.mipmapEnabled = false;
            glowImporter.SaveAndReimport();
        }

        Sprite triangle =
            AssetDatabase.LoadAssetAtPath<Sprite>(TrianglePath);

        if (triangle == null)
        {
            Debug.LogError("Triangle sprite is missing.");
            return;
        }

        Sprite glowRing =
            AssetDatabase.LoadAssetAtPath<Sprite>(GlowRingPath);

        if (glowRing == null)
        {
            Debug.LogError("Glow ring sprite is missing.");
            return;
        }

        GameObject playerContents =
            PrefabUtility.LoadPrefabContents(PlayerPrefabPath);

        PlayerIndicator indicator =
            playerContents.GetComponent<PlayerIndicator>();

        if (indicator == null)
            indicator = playerContents.AddComponent<PlayerIndicator>();

        SerializedObject indicatorData = new SerializedObject(indicator);
        indicatorData.FindProperty("glowSprite").objectReferenceValue = glowRing;
        indicatorData.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(
            playerContents, PlayerPrefabPath);

        PrefabUtility.UnloadPrefabContents(playerContents);

        UnityEngine.SceneManagement.Scene scene =
            EditorSceneManager.OpenScene(
                BattleScenePath,
                OpenSceneMode.Single);

        GameObject pointerObject = GameObject.Find("TargetPointer");

        if (pointerObject == null)
            pointerObject = new GameObject("TargetPointer");

        TargetPointer pointer =
            pointerObject.GetComponent<TargetPointer>();

        if (pointer == null)
            pointer = pointerObject.AddComponent<TargetPointer>();

        SerializedObject pointerData = new SerializedObject(pointer);
        pointerData.FindProperty("sprite").objectReferenceValue = triangle;
        pointerData.FindProperty("targetCamera").objectReferenceValue =
            Object.FindFirstObjectByType<Camera>();
        pointerData.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("Wired player and target indicators");
    }
}
