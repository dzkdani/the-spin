#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
public static class MenuCursorImporter
{
    private const string AssetPath = "Assets/Resources/MenuCursor.png";

    static MenuCursorImporter()
    {
        EditorApplication.delayCall += Apply;
    }

    [MenuItem("Tools/Fix Menu Cursor Import")]
    public static void Apply()
    {
        TextureImporter importer =
            AssetImporter.GetAtPath(AssetPath) as TextureImporter;

        if (importer == null)
            return;

        if (importer.textureType == TextureImporterType.Default &&
            importer.textureCompression == TextureImporterCompression.Uncompressed &&
            importer.alphaIsTransparency &&
            importer.isReadable &&
            !importer.mipmapEnabled)
            return;

        importer.textureType = TextureImporterType.Default;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.isReadable = true;
        importer.mipmapEnabled = false;

        importer.SaveAndReimport();
    }
}
#endif
