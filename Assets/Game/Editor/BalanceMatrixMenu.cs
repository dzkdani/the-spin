#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class BalanceMatrixMenu
{
    [MenuItem("Bey/Run Balance Matrix")]
    public static void Run()
    {
        ContentManifest manifest = AssetDatabase.LoadAssetAtPath<ContentManifest>(
            "Assets/Game/Datas/ContentManifest.asset");

        if (manifest == null)
        {
            Debug.LogError("ContentManifest.asset was not found.");
            return;
        }

        ContentRegistry.Initialize(manifest);

        var builds = BalanceCatalog.CreateReferenceBuilds();

        BalanceMatrix matrix = BalanceMatrix.Run(
            builds,
            25,
            manifest.matchupTable,
            manifest.combatTuning);

        Directory.CreateDirectory("Logs");
        File.WriteAllText("Logs/BalanceMatrix.csv", matrix.ToCsv());

        var outside = matrix.BuildsOutsideBand(0.4f, 0.6f);

        Debug.Log(
            $"Balance matrix: {builds.Count} builds, 25 seeds, CSV at Logs/BalanceMatrix.csv. " +
            $"Outside 40-60%: {outside.Count}.");

        for (int i = 0; i < outside.Count; i++)
            Debug.Log("Out of band: " + outside[i]);
    }
}
#endif
